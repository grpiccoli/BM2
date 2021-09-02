using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using BiblioMit.Models.Entities.Ads;

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
        public virtual IEnumerable<IdentityUserRole<string>> UserRoles { get; internal set; }
        /// <summary>
        /// Navigation property for the claims this user possesses.
        /// </summary>
        public virtual IEnumerable<IdentityUserClaim<string>> Claims { get; internal set; }
        public virtual ICollection<Banner> Banners { get; internal set; }
        //public virtual ICollection<PlataformaUser> Plataforma { get; } = new List<PlataformaUser>();
    }
}