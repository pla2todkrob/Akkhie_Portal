using Microsoft.Extensions.Options;
using Portal.Services.Interfaces;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Portal.Services.Models
{
    public class ActiveDirectoryService : IActiveDirectoryService
    {
        private readonly string _adServer;
        private readonly string _domain;
        private readonly string _adBindUser;
        private readonly string _adBindPassword;
        private readonly bool _useSecureConnection;

        public ActiveDirectoryService(IOptions<ActiveDirectorySettings> adSettingsOptions)
        {
            var settings = adSettingsOptions.Value;
            _adServer = settings.Server;
            _domain = settings.Domain;
            _adBindUser = settings.BindUser;
            _adBindPassword = settings.BindPassword;
            _useSecureConnection = settings.UseSecureConnection;
        }

        public async Task<bool> ValidateCredentials(string username, string password)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var identifier = new LdapDirectoryIdentifier(
                        server: _adServer,
                        portNumber: _useSecureConnection ? 636 : 389,
                        fullyQualifiedDnsHostName: _useSecureConnection,
                        connectionless: false
                    );

                    var credential = new NetworkCredential($"{username}@{_domain}", password);

                    using var connection = new LdapConnection(identifier)
                    {
                        Timeout = TimeSpan.FromSeconds(15)
                    };

                    connection.SessionOptions.ProtocolVersion = 3;
                    connection.AuthType = AuthType.Negotiate;

                    if (_useSecureConnection)
                    {
                        connection.SessionOptions.SecureSocketLayer = true;
                        connection.SessionOptions.VerifyServerCertificate = (conn, cert) =>
                        {
                            return true;
                        };
                    }

                    connection.Bind(credential);
                    return true;
                }
                catch (LdapException lex)
                {
                    Console.WriteLine($"LDAP Error (Code: {lex.ErrorCode}): {lex.Message}");
                    if (lex.ServerErrorMessage != null)
                    {
                        Console.WriteLine($"Server Error: {lex.ServerErrorMessage}");
                    }
                    return false;
                }
            });
        }

        public async Task<Dictionary<string, string>> GetUserProperties(string username)
        {
            return await GetUserPropertiesInternal(username, null).ConfigureAwait(false);
        }

        public async Task<Dictionary<string, string>> GetUserProperties(string username, string[] properties)
        {
            return await GetUserPropertiesInternal(username, properties).ConfigureAwait(false);
        }

        private async Task<Dictionary<string, string>> GetUserPropertiesInternal(string username, string[]? properties)
        {
            return await Task.Run(() =>
            {
                var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                try
                {
                    using var connection = CreateLdapConnection();

                    connection.Bind(new NetworkCredential($"{_adBindUser}@{_domain}", _adBindPassword));

                    string baseDn = string.Join(",", _domain.Split('.').Select(p => $"DC={p}"));
                    string filter = $"(&(objectClass=user)(sAMAccountName={username}))";

                    var request = new SearchRequest(
                        baseDn,
                        filter,
                        SearchScope.Subtree,
                        properties
                    );

                    var response = (SearchResponse)connection.SendRequest(request);

                    if (response.Entries.Count > 0)
                    {
                        var entry = response.Entries[0];

                        if (properties == null)
                        {
                            foreach (string attrName in entry.Attributes.AttributeNames)
                            {
                                var attr = entry.Attributes[attrName];
                                if (attr != null && attr.Count > 0)
                                {
                                    result[attrName] = attr.Count == 1
                                        ? attr[0].ToString()!
                                        : string.Join(", ", attr.Cast<object>().Select(v => v.ToString()));
                                }
                                else
                                {
                                    result[attrName] = string.Empty;
                                }
                            }
                        }
                        else
                        {
                            foreach (string prop in properties)
                            {
                                var attr = entry.Attributes[prop];
                                result[prop] = attr?.Count > 0 ? attr[0].ToString()! : string.Empty;
                            }
                        }
                    }
                }
                catch (LdapException lex)
                {
                    Console.WriteLine($"LDAP Error (Code: {lex.ErrorCode}): {lex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected Error: {ex.Message}");
                }

                return result;
            }).ConfigureAwait(false);
        }

        private LdapConnection CreateLdapConnection()
        {
            var identifier = new LdapDirectoryIdentifier(
                _adServer,
                _useSecureConnection ? 636 : 389,
                _useSecureConnection,
                false
            );

            var connection = new LdapConnection(identifier)
            {
                Timeout = TimeSpan.FromSeconds(15),
                AuthType = AuthType.Negotiate
            };

            connection.SessionOptions.ProtocolVersion = 3;

            if (_useSecureConnection)
            {
                connection.SessionOptions.SecureSocketLayer = true;
                connection.SessionOptions.VerifyServerCertificate = (conn, cert) => true;
            }

            return connection;
        }
    }
}
