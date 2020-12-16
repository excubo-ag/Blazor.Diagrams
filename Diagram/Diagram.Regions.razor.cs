namespace Excubo.Blazor.Diagrams
{
    public partial class Diagram
    {
        internal Links Links { get; set; }
        internal Nodes Nodes { get; set; }
        private NavigationSettings navigation_settings;
        internal NavigationSettings NavigationSettings
        {
            get
            {
                if (navigation_settings == null)
                {
                    navigation_settings = new NavigationSettings { Diagram = this };
                }
                return navigation_settings;
            }
            set
            {
                navigation_settings = value;
            }
        }
        internal NodeLibrary NodeLibrary { get; set; }
        internal LinkSelectorSettings LinkSelectorSettings { get; set; }
        internal OverviewSettings OverviewSettings { get; set; }
        internal AutoLayoutSettings AutoLayoutSettings { get; set; }
        internal void UpdateOverview(bool just_pan_or_zoom = false)
        {
            Overview?.TriggerUpdate(just_pan_or_zoom);
        }
    }
}