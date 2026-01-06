using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Utils;

public static class PasswordUtil
{
    private const int HashingRound = 10;

    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghijkmnopqrstuvwxyz";
    private const string Digits = "1234567890";
    private const string Symbols = "!@#$%^&()*+-";

    public static async Task<string> HashPassword(string rawPassword)
    {
        return await Task.Run(() => BCrypt.Net.BCrypt.HashPassword(rawPassword, workFactor: HashingRound));
    }

    public static async Task<bool> VerifyPassword(string rawPassword, string hashedPassword)
    {
        return await Task.Run(() => BCrypt.Net.BCrypt.Verify(rawPassword, hashedPassword));
    }
    public static string GenerateDefaultPassword(int length = 12)
    {
        if (length < 8)
            throw new ArgumentException("Password length must be at least 8.");

        var allChars = Upper + Lower + Digits + Symbols;
        var password = new StringBuilder();

        password.Append(GetRandomChar(Upper));
        password.Append(GetRandomChar(Lower));
        password.Append(GetRandomChar(Digits));
        password.Append(GetRandomChar(Symbols));

        for (int i = password.Length; i < length; i++)
        {
            password.Append(GetRandomChar(allChars));
        }

        return Shuffle(password.ToString());
    }

    private static char GetRandomChar(string chars)
    {
        var buffer = new byte[1];
        RandomNumberGenerator.Fill(buffer);
        return chars[buffer[0] % chars.Length];
    }

    private static string Shuffle(string input)
    {
        var chars = input.ToCharArray();
        for (int i = chars.Length - 1; i > 0; i--)
        {
            var buffer = new byte[1];
            RandomNumberGenerator.Fill(buffer);
            int j = buffer[0] % (i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
        return new string(chars);
    }

}
