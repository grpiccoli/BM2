using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.VM
{
    public enum Typep
    {
        [Display(Name = "Thesis")]
        Thesis,
        [Display(Name = "Article")]
        Article,
        [Display(Name = "Book")]
        Book,
        [Display(Name = "Unknown")]
        Unknown,
        [Display(Name = "Patent")]
        Patent,
        [Display(Name = "Project")]
        Project
    }
    public class PublicationVM
    {
        //Parent
        public Company Company { get; set; }
        //Attributes
        [Display(Name = "Type")]
        public Typep Typep { get; set; }
        [Display(Name = "Source")]
        public string Source { get; set; }
        public Uri Uri { get; set; }
        [Display(Name = "Title")]
        public string Title { get; set; }
        [Display(Name = "Abstract")]
        public string Abstract { get; set; }
        [Display(Name = "Journal")]
        public string Journal { get; set; }
        [Display(Name = "DOI")]
        public string DOI { get; set; }
        [Display(Name = "Year")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy}")]
        public DateTime Date { get; set; }
        [Display(Name = "Authors")]
        public IEnumerable<AuthorVM> Authors { get; set; }
    }
}
