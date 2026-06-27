// src/Application/Interfaces/Interface/ISmtpService.cs
using Application.Common;

namespace Application.Interfaces.Interface;

public interface ISmtpService
{
    Task<Result> SendEmailAsync(string to, string subject, string body);
}
