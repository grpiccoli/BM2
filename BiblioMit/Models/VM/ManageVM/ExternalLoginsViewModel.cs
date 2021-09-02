using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace BiblioMit.Models.ManageViewModels
{
    public class ExternalLoginsViewModel
    {
        public Collection<UserLoginInfo> CurrentLogins { get; } = new Collection<UserLoginInfo>();

        public Collection<AuthenticationScheme> OtherLogins { get; } = new Collection<AuthenticationScheme>();

        public bool ShowRemoveButton { get; set; }

        public string StatusMessage { get; set; }
    }
}
