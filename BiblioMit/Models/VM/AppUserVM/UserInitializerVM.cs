using System;
using System.Collections.Generic;

namespace BiblioMit.Models.ViewModels
{
    public class UserInitializerVM
    {
        public UserInitializerVM(IEnumerable<string> roles, IEnumerable<string> claims)
        {
            Roles = roles;
            Claims = claims;
        }
        public string Name { get; set; }
        public IEnumerable<string> Roles { get; internal set; }
        public IEnumerable<string> Claims { get; internal set; }
        public string Email { get; set; }
        public string Key { get; set; }
        //public List<Plataforma> Plataforma { get; } = new List<Plataforma>();
        public Uri ImageUri { get; set; }
        public int? Rating { get; set; }
    }
}
