﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    // Add profile data for application users by adding properties to the AppUser class
    public class ApplicationUser : IdentityUser
    {
        [RegularExpression(@"^.*$")]
        public string Name { get; set; }
        public string Last { get; set; }
        public int Rating { get; set; }
        public Uri ProfileImageUrl { get; set; }
        public DateTime MemberSince { get; set; }
        public bool IsActive { get; set; }
        /// <summary>
        /// Navigation property for the roles this user belongs to.
        /// </summary>
        public virtual ICollection<IdentityUserRole<string>> UserRoles { get; } = new List<IdentityUserRole<string>>();
        /// <summary>
        /// Navigation property for the claims this user possesses.
        /// </summary>
        public virtual ICollection<IdentityUserClaim<string>> Claims { get; } = new List<IdentityUserClaim<string>>();
        //public virtual ICollection<PlataformaUser> Plataforma { get; } = new List<PlataformaUser>();
    }
}