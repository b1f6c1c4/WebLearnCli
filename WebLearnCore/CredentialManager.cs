using CredentialManagement;

namespace WebLearnCore
{
    public class WebLearnCredential
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }

    public static class CredentialManager
    {
        private const string Target = "WebLearn";

        private static WebLearnCredential Convert(Credential cred) =>
            new WebLearnCredential { Username = cred.Username, Password = cred.Password };

        private static Credential CredentialTemplate() =>
            new Credential
                {
                    Target = Target,
                    PersistanceType = PersistanceType.Enterprise,
                    Type = CredentialType.DomainVisiblePassword
                };

        public static bool DropCredential() => CredentialTemplate().Delete();

        private static WebLearnCredential PromptForCredential()
        {
            var prompt = new XPPrompt
                             {
                                 Target = Target,
                                 Persist = true
                             };
            if (prompt.ShowDialog() != DialogResult.OK)
                return null;

            var cred = CredentialTemplate();
            cred.Username = prompt.Username.Split(new[] { '\\' }, 2)[1];
            cred.Password = prompt.Password;

            cred.Save();
            return Convert(cred);
        }

        public static WebLearnCredential TryGetCredential(bool force = false)
        {
            if (force)
            {
                DropCredential();
                return PromptForCredential();
            }

            var cred = CredentialTemplate();
            if (cred.Exists())
                return cred.Load() ? Convert(cred) : null;
            return PromptForCredential();
        }
    }
}
