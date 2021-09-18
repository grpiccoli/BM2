namespace BiblioMit.Models
{
    public class BootstrapModel
    {
        public string Id { get; set; }
        public string AreaLabeledId { get; set; }
        public ModalSize Size { get; set; }
        public string Message { get; set; }
        public string ModalSizeClass
        {
            get
            {
                return Size switch
                {
                    ModalSize.Small => "modal-sm",
                    ModalSize.Large => "modal-lg",
                    _ => "",
                };
            }
        }        
    }
}