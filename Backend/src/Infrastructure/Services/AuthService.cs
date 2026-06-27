using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Application.Common;
using Application.Common.Settings;
using Application.DTOs.AuthDTOs;
using Application.Interfaces.Interface;
using Application.Interfaces.Repository;
using Domain.Entities;
using Domain.Enums;
using Google.Apis.Auth;
using Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private const string InvalidLoginMessage = "Неверный email или пароль";
    private const string ExpiredSessionMessage = "Сессия истекла";
    private const string InvalidResetCodeMessage = "Неверный или истекший код сброса";
    private const int ResetCodeExpiryMinutes = 15;
    private const int MaxEmailLength = 255;
    private const int MaxNameLength = 50;
    private const int MaxPasswordLength = 128;
    private const int MinPasswordLength = 8;

    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private readonly IUserRepository _userRepository;
    private readonly ISmtpService _smtpService;
    private readonly JwtSettings _jwtSettings;
    private readonly GoogleAuthSettings _googleAuthSettings;
    private readonly PasswordHasher _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        ISmtpService smtpService,
        IOptions<JwtSettings> jwtOptions,
        IOptions<GoogleAuthSettings> googleAuthOptions,
        PasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _smtpService = smtpService;
        _jwtSettings = jwtOptions.Value;
        _googleAuthSettings = googleAuthOptions.Value;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AuthResultDto>> RegisterAsync(RegisterDto dto)
    {
        if (dto == null)
        {
            return Result<AuthResultDto>.Failure("Данные запроса отсутствуют");
        }

        Result<ValidatedRegisterData> validationResult = ValidateRegisterData(dto);

        if (validationResult.IsFailure)
        {
            return Result<AuthResultDto>.Failure(validationResult.Error);
        }

        ValidatedRegisterData validatedData = validationResult.Value;
        User? existingUser = await _userRepository.GetByEmailAsync(validatedData.Email);

        if (existingUser != null)
        {
            return Result<AuthResultDto>.Failure("Пользователь с таким email уже зарегистрирован");
        }

        User user = new User
        {
            Email = validatedData.Email,
            PasswordHash = _passwordHasher.HashPassword(validatedData.Password),
            FirstName = validatedData.FirstName,
            LastName = validatedData.LastName,
            Role = validatedData.Role,
            CreatedAt = DateTime.UtcNow
        };

        TokenPair tokenPair = GenerateTokenPair(user);
        user.RefreshToken = HashRefreshToken(tokenPair.RefreshToken);
        user.RefreshTokenExpires = tokenPair.RefreshTokenExpiration;

        await _userRepository.AddAsync(user);

        return Result<AuthResultDto>.Success(MapToAuthResultDto(user, tokenPair));
    }

    public async Task<Result<AuthResultDto>> LoginAsync(LoginDto dto)
    {
        if (dto == null)
        {
            return Result<AuthResultDto>.Failure(InvalidLoginMessage);
        }

        Result<ValidatedLoginData> validationResult = ValidateLoginData(dto);

        if (validationResult.IsFailure)
        {
            return Result<AuthResultDto>.Failure(validationResult.Error);
        }

        ValidatedLoginData validatedData = validationResult.Value;
        User? user = await _userRepository.GetByEmailAsync(validatedData.Email);

        if (user == null)
        {
            return Result<AuthResultDto>.Failure(InvalidLoginMessage);
        }

        bool passwordVerified = _passwordHasher.VerifyPassword(validatedData.Password, user.PasswordHash);

        if (!passwordVerified)
        {
            return Result<AuthResultDto>.Failure(InvalidLoginMessage);
        }

        TokenPair tokenPair = GenerateTokenPair(user);
        user.RefreshToken = HashRefreshToken(tokenPair.RefreshToken);
        user.RefreshTokenExpires = tokenPair.RefreshTokenExpiration;

        await _userRepository.UpdateAsync(user);

        return Result<AuthResultDto>.Success(MapToAuthResultDto(user, tokenPair));
    }

    public async Task<Result<AuthResultDto>> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result<AuthResultDto>.Failure(ExpiredSessionMessage);
        }

        string trimmedRefreshToken = refreshToken.Trim();

        if (ContainsDangerousCharacters(trimmedRefreshToken))
        {
            return Result<AuthResultDto>.Failure(ExpiredSessionMessage);
        }

        User? user = await _userRepository.GetByRefreshTokenAsync(trimmedRefreshToken);

        if (user == null)
        {
            return Result<AuthResultDto>.Failure(ExpiredSessionMessage);
        }

        if (!user.RefreshTokenExpires.HasValue)
        {
            return Result<AuthResultDto>.Failure(ExpiredSessionMessage);
        }

        if (DateTime.UtcNow > user.RefreshTokenExpires.Value)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpires = null;
            await _userRepository.UpdateAsync(user);

            return Result<AuthResultDto>.Failure(ExpiredSessionMessage);
        }

        TokenPair tokenPair = GenerateTokenPair(user);
        user.RefreshToken = HashRefreshToken(tokenPair.RefreshToken);
        user.RefreshTokenExpires = tokenPair.RefreshTokenExpiration;

        await _userRepository.UpdateAsync(user);

        return Result<AuthResultDto>.Success(MapToAuthResultDto(user, tokenPair));
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        if (dto == null)
        {
            return Result.Success();
        }

        Result<string> emailResult = ValidateEmail(dto.Email);

        if (emailResult.IsFailure)
        {
            return Result.Success();
        }

        User? user = await _userRepository.GetByEmailAsync(emailResult.Value);

        if (user == null)
        {
            return Result.Success();
        }

        string resetCode = GenerateResetCode();
        user.ResetCode = resetCode;
        user.ResetCodeExpires = DateTime.UtcNow.AddMinutes(ResetCodeExpiryMinutes);

        await _userRepository.UpdateAsync(user);

        string subject = "Код сброса пароля TimeTableMSU";
        string body = BuildResetPasswordEmailBody(user.FirstName, resetCode);
        Result sendResult = await _smtpService.SendEmailAsync(user.Email, subject, body);

        if (sendResult.IsFailure)
        {
            return Result.Failure(sendResult.Error);
        }

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (dto == null)
        {
            return Result.Failure("Данные запроса отсутствуют");
        }

        Result<ValidatedResetPasswordData> validationResult = ValidateResetPasswordData(dto);

        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }

        ValidatedResetPasswordData validatedData = validationResult.Value;
        User? user = await _userRepository.GetByEmailAsync(validatedData.Email);

        if (user == null)
        {
            return Result.Failure(InvalidResetCodeMessage);
        }

        if (string.IsNullOrWhiteSpace(user.ResetCode))
        {
            return Result.Failure(InvalidResetCodeMessage);
        }

        if (!user.ResetCodeExpires.HasValue)
        {
            return Result.Failure(InvalidResetCodeMessage);
        }

        if (DateTime.UtcNow > user.ResetCodeExpires.Value)
        {
            user.ResetCode = null;
            user.ResetCodeExpires = null;
            await _userRepository.UpdateAsync(user);

            return Result.Failure(InvalidResetCodeMessage);
        }

        if (!string.Equals(user.ResetCode, validatedData.Code, StringComparison.Ordinal))
        {
            return Result.Failure(InvalidResetCodeMessage);
        }

        user.PasswordHash = _passwordHasher.HashPassword(validatedData.NewPassword);
        user.ResetCode = null;
        user.ResetCodeExpires = null;
        user.RefreshToken = null;
        user.RefreshTokenExpires = null;

        await _userRepository.UpdateAsync(user);

        return Result.Success();
    }

    public async Task<Result<AuthResultDto>> ExternalLoginAsync(ExternalLoginDto dto)
    {
        if (dto == null)
        {
            return Result<AuthResultDto>.Failure("Данные запроса отсутствуют");
        }

        Result<ValidatedExternalLoginData> validationResult = ValidateExternalLoginData(dto);

        if (validationResult.IsFailure)
        {
            return Result<AuthResultDto>.Failure(validationResult.Error);
        }

        ValidatedExternalLoginData validatedData = validationResult.Value;
        Result<ExternalUserData> externalUserResult = await ResolveExternalUserAsync(validatedData);

        if (externalUserResult.IsFailure)
        {
            return Result<AuthResultDto>.Failure(externalUserResult.Error);
        }

        ExternalUserData externalUserData = externalUserResult.Value;
        User? user = await _userRepository.GetByEmailAsync(externalUserData.Email);

        if (user == null)
        {
            user = new User
            {
                Email = externalUserData.Email,
                PasswordHash = _passwordHasher.HashPassword(GenerateRefreshToken()),
                FirstName = externalUserData.FirstName,
                LastName = externalUserData.LastName,
                Role = UserRole.Student,
                ExternalProvider = validatedData.Provider,
                ExternalProviderId = externalUserData.ProviderId,
                CreatedAt = DateTime.UtcNow
            };

            TokenPair createdTokenPair = GenerateTokenPair(user);
            user.RefreshToken = HashRefreshToken(createdTokenPair.RefreshToken);
            user.RefreshTokenExpires = createdTokenPair.RefreshTokenExpiration;

            await _userRepository.AddAsync(user);

            return Result<AuthResultDto>.Success(MapToAuthResultDto(user, createdTokenPair));
        }

        TokenPair tokenPair = GenerateTokenPair(user);
        user.ExternalProvider = validatedData.Provider;
        user.ExternalProviderId = externalUserData.ProviderId;
        user.RefreshToken = HashRefreshToken(tokenPair.RefreshToken);
        user.RefreshTokenExpires = tokenPair.RefreshTokenExpiration;

        await _userRepository.UpdateAsync(user);

        return Result<AuthResultDto>.Success(MapToAuthResultDto(user, tokenPair));
    }

    private Result<ValidatedRegisterData> ValidateRegisterData(RegisterDto dto)
    {
        Result<string> emailResult = ValidateEmail(dto.Email);

        if (emailResult.IsFailure)
        {
            return Result<ValidatedRegisterData>.Failure(emailResult.Error);
        }

        Result<string> firstNameResult = ValidateName(dto.FirstName, "Имя");

        if (firstNameResult.IsFailure)
        {
            return Result<ValidatedRegisterData>.Failure(firstNameResult.Error);
        }

        Result<string> lastNameResult = ValidateName(dto.LastName, "Фамилия");

        if (lastNameResult.IsFailure)
        {
            return Result<ValidatedRegisterData>.Failure(lastNameResult.Error);
        }

        Result passwordResult = ValidatePassword(dto.Password);

        if (passwordResult.IsFailure)
        {
            return Result<ValidatedRegisterData>.Failure(passwordResult.Error);
        }

        if (string.IsNullOrWhiteSpace(dto.ConfirmPassword))
        {
            return Result<ValidatedRegisterData>.Failure("Подтверждение пароля отсутствует");
        }

        if (!string.Equals(dto.Password, dto.ConfirmPassword, StringComparison.Ordinal))
        {
            return Result<ValidatedRegisterData>.Failure("Пароль и подтверждение пароля не совпадают");
        }

        Result<UserRole> roleResult = ParseRole(dto.Role);

        if (roleResult.IsFailure)
        {
            return Result<ValidatedRegisterData>.Failure(roleResult.Error);
        }

        ValidatedRegisterData data = new ValidatedRegisterData
        {
            Email = emailResult.Value,
            Password = dto.Password,
            FirstName = firstNameResult.Value,
            LastName = lastNameResult.Value,
            Role = roleResult.Value
        };

        return Result<ValidatedRegisterData>.Success(data);
    }

    private static Result<ValidatedLoginData> ValidateLoginData(LoginDto dto)
    {
        Result<string> emailResult = ValidateEmail(dto.Email);

        if (emailResult.IsFailure)
        {
            return Result<ValidatedLoginData>.Failure(InvalidLoginMessage);
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return Result<ValidatedLoginData>.Failure(InvalidLoginMessage);
        }

        string password = dto.Password.Trim();

        if (ContainsDangerousCharacters(password))
        {
            return Result<ValidatedLoginData>.Failure(InvalidLoginMessage);
        }

        ValidatedLoginData data = new ValidatedLoginData
        {
            Email = emailResult.Value,
            Password = password
        };

        return Result<ValidatedLoginData>.Success(data);
    }

    private static Result<ValidatedResetPasswordData> ValidateResetPasswordData(ResetPasswordDto dto)
    {
        Result<string> emailResult = ValidateEmail(dto.Email);

        if (emailResult.IsFailure)
        {
            return Result<ValidatedResetPasswordData>.Failure(emailResult.Error);
        }

        if (string.IsNullOrWhiteSpace(dto.Code))
        {
            return Result<ValidatedResetPasswordData>.Failure("Код сброса отсутствует");
        }

        string code = dto.Code.Trim();

        if (ContainsDangerousCharacters(code))
        {
            return Result<ValidatedResetPasswordData>.Failure("Код сброса содержит недопустимые символы");
        }

        if (code.Length != 6)
        {
            return Result<ValidatedResetPasswordData>.Failure("Код сброса должен состоять из 6 цифр");
        }

        foreach (char symbol in code)
        {
            if (!char.IsDigit(symbol))
            {
                return Result<ValidatedResetPasswordData>.Failure("Код сброса должен состоять из 6 цифр");
            }
        }

        Result passwordResult = ValidatePassword(dto.NewPassword);

        if (passwordResult.IsFailure)
        {
            return Result<ValidatedResetPasswordData>.Failure(passwordResult.Error);
        }

        if (string.IsNullOrWhiteSpace(dto.ConfirmNewPassword))
        {
            return Result<ValidatedResetPasswordData>.Failure("Подтверждение нового пароля отсутствует");
        }

        if (!string.Equals(dto.NewPassword, dto.ConfirmNewPassword, StringComparison.Ordinal))
        {
            return Result<ValidatedResetPasswordData>.Failure("Новый пароль и подтверждение пароля не совпадают");
        }

        ValidatedResetPasswordData data = new ValidatedResetPasswordData
        {
            Email = emailResult.Value,
            Code = code,
            NewPassword = dto.NewPassword
        };

        return Result<ValidatedResetPasswordData>.Success(data);
    }

    private static Result<ValidatedExternalLoginData> ValidateExternalLoginData(ExternalLoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Provider))
        {
            return Result<ValidatedExternalLoginData>.Failure("Провайдер авторизации отсутствует");
        }

        string provider = dto.Provider.Trim();

        if (ContainsDangerousCharacters(provider))
        {
            return Result<ValidatedExternalLoginData>.Failure("Провайдер авторизации содержит недопустимые символы");
        }

        if (!string.Equals(provider, "Google", StringComparison.OrdinalIgnoreCase))
        {
            return Result<ValidatedExternalLoginData>.Failure("Поддерживается только Google OAuth2");
        }

        if (string.IsNullOrWhiteSpace(dto.ProviderToken))
        {
            return Result<ValidatedExternalLoginData>.Failure("Токен провайдера отсутствует");
        }

        string providerToken = dto.ProviderToken.Trim();

        if (ContainsDangerousCharacters(providerToken))
        {
            return Result<ValidatedExternalLoginData>.Failure("Токен провайдера содержит недопустимые символы");
        }

        ValidatedExternalLoginData data = new ValidatedExternalLoginData
        {
            Provider = "Google",
            ProviderToken = providerToken
        };

        return Result<ValidatedExternalLoginData>.Success(data);
    }

    private async Task<Result<ExternalUserData>> ResolveExternalUserAsync(ValidatedExternalLoginData data)
    {
        if (string.IsNullOrWhiteSpace(_googleAuthSettings.ClientId))
        {
            return Result<ExternalUserData>.Failure("Google OAuth2 ClientId не настроен");
        }

        try
        {
            GoogleJsonWebSignature.ValidationSettings validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleAuthSettings.ClientId }
            };

            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(data.ProviderToken, validationSettings);
            Result<string> emailResult = ValidateEmail(payload.Email);

            if (emailResult.IsFailure)
            {
                return Result<ExternalUserData>.Failure("Google вернул некорректный email");
            }

            string firstName = NormalizeExternalName(payload.GivenName, "Google");
            string lastName = NormalizeExternalName(payload.FamilyName, "User");
            string providerId = string.IsNullOrWhiteSpace(payload.Subject) ? emailResult.Value : payload.Subject.Trim();

            ExternalUserData externalUserData = new ExternalUserData
            {
                Email = emailResult.Value,
                FirstName = firstName,
                LastName = lastName,
                ProviderId = providerId
            };

            return Result<ExternalUserData>.Success(externalUserData);
        }
        catch
        {
            return Result<ExternalUserData>.Failure("Не удалось проверить Google OAuth2 токен");
        }
    }

    private TokenPair GenerateTokenPair(User user)
    {
        ValidateJwtSettings();

        string accessToken = GenerateAccessToken(user);
        string refreshToken = GenerateRefreshToken();
        DateTime refreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);

        TokenPair tokenPair = new TokenPair
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = refreshTokenExpiration
        };

        return tokenPair;
    }

    private string GenerateAccessToken(User user)
    {
        byte[] secretBytes = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(secretBytes);
        SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        string fullName = BuildFullName(user.FirstName, user.LastName);

        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.Name, fullName),
            new Claim("FullName", fullName)
        };

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            signingCredentials: signingCredentials);

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

        return handler.WriteToken(token);
    }

    private void ValidateJwtSettings()
    {
        if (string.IsNullOrWhiteSpace(_jwtSettings.Secret))
        {
            throw new InvalidOperationException("Jwt:Secret is not configured.");
        }

        byte[] secretBytes = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

        if (secretBytes.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Secret must be at least 32 bytes long.");
        }

        if (string.IsNullOrWhiteSpace(_jwtSettings.Issuer))
        {
            throw new InvalidOperationException("Jwt:Issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_jwtSettings.Audience))
        {
            throw new InvalidOperationException("Jwt:Audience is not configured.");
        }

        if (_jwtSettings.AccessTokenExpiryMinutes <= 0)
        {
            throw new InvalidOperationException("Jwt:AccessTokenExpiryMinutes must be greater than zero.");
        }

        if (_jwtSettings.RefreshTokenExpiryDays <= 0)
        {
            throw new InvalidOperationException("Jwt:RefreshTokenExpiryDays must be greater than zero.");
        }
    }

    private static string GenerateRefreshToken()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }

    private static string HashRefreshToken(string refreshToken)
    {
        byte[] tokenBytes = Encoding.UTF8.GetBytes(refreshToken);
        byte[] hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToBase64String(hashBytes);
    }

    private static string GenerateResetCode()
    {
        int value = RandomNumberGenerator.GetInt32(100000, 1000000);

        return value.ToString();
    }

    private static string BuildResetPasswordEmailBody(string firstName, string resetCode)
    {
        return $"Здравствуйте, {firstName}!\n\nВаш код для сброса пароля в TimeTableMSU: {resetCode}\n\nКод действует 15 минут. Если вы не запрашивали сброс пароля, просто проигнорируйте это письмо.";
    }

    private static AuthResultDto MapToAuthResultDto(User user, TokenPair tokenPair)
    {
        AuthResultDto dto = new AuthResultDto
        {
            AccessToken = tokenPair.AccessToken,
            RefreshToken = tokenPair.RefreshToken,
            RefreshTokenExpiration = tokenPair.RefreshTokenExpiration,
            Email = user.Email,
            Role = user.Role.ToString(),
            FullName = BuildFullName(user.FirstName, user.LastName)
        };

        return dto;
    }

    private static Result<string> ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<string>.Failure("Email не может быть пустым");
        }

        string trimmedEmail = email.Trim();

        if (trimmedEmail.Length > MaxEmailLength)
        {
            return Result<string>.Failure("Email не должен превышать 255 символов");
        }

        if (ContainsDangerousCharacters(trimmedEmail))
        {
            return Result<string>.Failure("Email содержит недопустимые символы");
        }

        if (!EmailRegex.IsMatch(trimmedEmail))
        {
            return Result<string>.Failure("Некорректный формат Email");
        }

        return Result<string>.Success(trimmedEmail.ToLowerInvariant());
    }

    private static Result<string> ValidateName(string? name, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<string>.Failure($"{fieldName} не может быть пустым");
        }

        string trimmedName = name.Trim();

        if (trimmedName.Length < 2)
        {
            return Result<string>.Failure($"{fieldName} должно быть от 2 до 50 символов");
        }

        if (trimmedName.Length > MaxNameLength)
        {
            return Result<string>.Failure($"{fieldName} должно быть от 2 до 50 символов");
        }

        if (ContainsDangerousCharacters(trimmedName))
        {
            return Result<string>.Failure($"{fieldName} содержит недопустимые символы");
        }

        foreach (char symbol in trimmedName)
        {
            if (symbol == '-')
            {
                continue;
            }

            if (symbol == ' ')
            {
                continue;
            }

            if (!char.IsLetter(symbol))
            {
                return Result<string>.Failure($"{fieldName} содержит недопустимые символы");
            }
        }

        return Result<string>.Success(trimmedName);
    }

    private static Result ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return Result.Failure("Пароль не может быть пустым");
        }

        if (password.Length < MinPasswordLength)
        {
            return Result.Failure("Пароль должен содержать минимум 8 символов");
        }

        if (password.Length > MaxPasswordLength)
        {
            return Result.Failure("Пароль не должен превышать 128 символов");
        }

        if (ContainsDangerousCharacters(password))
        {
            return Result.Failure("Пароль содержит недопустимые символы");
        }

        bool hasUpper = false;
        bool hasLower = false;
        bool hasDigit = false;
        bool hasSpecial = false;

        foreach (char symbol in password)
        {
            if (char.IsUpper(symbol))
            {
                hasUpper = true;
            }
            else if (char.IsLower(symbol))
            {
                hasLower = true;
            }
            else if (char.IsDigit(symbol))
            {
                hasDigit = true;
            }
            else
            {
                hasSpecial = true;
            }
        }

        if (!hasUpper)
        {
            return Result.Failure("Пароль должен содержать минимум одну заглавную букву");
        }

        if (!hasLower)
        {
            return Result.Failure("Пароль должен содержать минимум одну строчную букву");
        }

        if (!hasDigit)
        {
            return Result.Failure("Пароль должен содержать минимум одну цифру");
        }

        if (!hasSpecial)
        {
            return Result.Failure("Пароль должен содержать минимум один спецсимвол");
        }

        return Result.Success();
    }

    private static Result<UserRole> ParseRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return Result<UserRole>.Failure("Роль пользователя отсутствует");
        }

        string trimmedRole = role.Trim();

        if (ContainsDangerousCharacters(trimmedRole))
        {
            return Result<UserRole>.Failure("Роль содержит недопустимые символы");
        }

        if (Enum.TryParse(trimmedRole, true, out UserRole parsedRole))
        {
            if (Enum.IsDefined(typeof(UserRole), parsedRole))
            {
                return Result<UserRole>.Success(parsedRole);
            }
        }

        return Result<UserRole>.Failure("Недопустимая роль пользователя");
    }

    private static string NormalizeExternalName(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        string trimmedValue = value.Trim();

        if (ContainsDangerousCharacters(trimmedValue))
        {
            return fallback;
        }

        if (trimmedValue.Length > MaxNameLength)
        {
            return trimmedValue.Substring(0, MaxNameLength);
        }

        return trimmedValue;
    }

    private static string BuildFullName(string firstName, string lastName)
    {
        return $"{lastName} {firstName}";
    }

    private static bool ContainsDangerousCharacters(string value)
    {
        if (value.Contains('<'))
        {
            return true;
        }

        if (value.Contains('>'))
        {
            return true;
        }

        if (value.Contains(';'))
        {
            return true;
        }

        if (value.IndexOf("<script", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        if (value.IndexOf("</script", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        if (value.IndexOf("script>", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        if (value.IndexOf("javascript:", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        if (value.IndexOf("onerror=", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        if (value.IndexOf("onload=", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        return false;
    }

    private class ValidatedRegisterData
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public UserRole Role { get; set; }
    }

    private class ValidatedLoginData
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    private class ValidatedResetPasswordData
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

    private class ValidatedExternalLoginData
    {
        public string Provider { get; set; } = null!;
        public string ProviderToken { get; set; } = null!;
    }

    private class ExternalUserData
    {
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string ProviderId { get; set; } = null!;
    }

    private class TokenPair
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
