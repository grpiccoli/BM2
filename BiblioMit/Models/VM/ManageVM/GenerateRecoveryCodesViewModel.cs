using System.Collections.Generic;

namespace BiblioMit.Models.ManageViewModels
{
    public class GenerateRecoveryCodesViewModel
    {
        public GenerateRecoveryCodesViewModel(IEnumerable<string> recoveryCodes)
        {
            RecoveryCodes = recoveryCodes;
        }
        public IEnumerable<string> RecoveryCodes { get; internal set; }
    }
}
