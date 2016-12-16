using CredentialManagement;

namespace WebLearnCli
{
    internal static class CredentialManager
    {
        private const string Target = "WebLearn";

        private static Credential CredentialTemplate() =>
            new Credential
                {
                    Target = Target,
                    PersistanceType = PersistanceType.Enterprise,
                    Type = CredentialType.DomainVisiblePassword
                };

        public static void DropCredential()
        {
            var cred = CredentialTemplate();
            if (cred.Exists())
                cred.Delete();
        }

        private static Credential PromptForCredential()
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
            return cred;
        }

        public static Credential TryGetCredential(bool force = false)
        {
            if (force)
            {
                DropCredential();
                return PromptForCredential();
            }

            var cred = CredentialTemplate();
            if (cred.Exists())
                return cred.Load() ? cred : null;
            return PromptForCredential();
        }
    }
}
