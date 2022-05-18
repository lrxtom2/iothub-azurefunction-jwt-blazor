using JWT.Algorithms;
using JWT.Builder;
using System;
using System.Collections.Generic;

namespace func_openapi.Helper
{
    public class JWTValidateHelper
    {
        public bool IsValid { get; }
        public string Username { get; }
        public string Name { get; }
        public string Role { get; }

        public JWTValidateHelper(string authorizationHeader)
        {
            // Check if we can decode the header.
            IDictionary<string, object> claims = null;
            try
            {
                if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer"))
                {
                    authorizationHeader = authorizationHeader[7..];
                }
                else
                {
                    IsValid = false;

                    return;
                }

                // Validate the token and decode the claims.
                claims = new JwtBuilder()
                    .WithAlgorithm(new HMACSHA256Algorithm())
                    .WithSecret("authkey1233456767890")
                    .MustVerifySignature()
                    .Decode<IDictionary<string, object>>(authorizationHeader);
            }
            catch (Exception)
            {
                IsValid = false;

                return;
            }

            // Check if we have user claim.
            if (!claims.ContainsKey("username"))
            {
                IsValid = false;

                return;
            }

            // Check if expire
            if (claims.ContainsKey("exp"))
            {
                var expDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(claims["exp"].ToString())).UtcDateTime;

                if (expDate < DateTime.UtcNow)
                {
                    IsValid = false;
                    return;
                }
            }

            IsValid = true;
            Username = Convert.ToString(claims["username"]);
            Name = Convert.ToString(claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]);
            Role = Convert.ToString(claims["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]);
        }
    }
}