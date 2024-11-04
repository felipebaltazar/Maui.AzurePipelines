using PipelineApproval.Presentation;

namespace PipelineApproval
{

    /// <summary>
    /// This class is never actually executed, but when Xamarin linking is enabled it does how to ensure types and properties
    /// are preserved in the deployed app
    /// </summary>
    public class LinkerPleaseInclude
    {
        public void Include()
        {
            var s = FontAwesome.A;
            s = FontAwesome.AddressBook;
            s = FontAwesome.AddressCard;
            s = FontAwesome.ChevronDown;
        }
    }
}
