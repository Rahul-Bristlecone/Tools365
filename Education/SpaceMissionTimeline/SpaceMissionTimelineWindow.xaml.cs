using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.Graphics;

namespace Tools365.Education.SpaceMissionTimeline;

public sealed partial class SpaceMissionTimelineWindow : Window
{
    private const string AllCategories = "All categories";

    private static readonly SpaceMissionEvent[] Events =
    [
        new("4 October 1957", "Global space milestones", "Sputnik 1",
            "The Soviet Union launched Sputnik 1, the first artificial satellite to orbit Earth, beginning the space age."),
        new("12 April 1961", "Global space milestones", "First human in space",
            "Soviet cosmonaut Yuri Gagarin completed one orbit of Earth aboard Vostok 1."),
        new("20 July 1969", "Global space milestones", "Apollo 11 Moon landing",
            "NASA astronauts Neil Armstrong and Buzz Aldrin became the first humans to walk on the Moon."),
        new("19 April 1975", "ISRO foundations", "Aryabhata satellite launched",
            "India's first satellite, Aryabhata, was launched into orbit by a Soviet launch vehicle, advancing India's early space research programme."),
        new("18 July 1980", "Launch vehicles", "SLV-3 places Rohini in orbit",
            "The SLV-3 successfully placed the Rohini satellite RS-1 in orbit, making India one of the countries with independent satellite launch capability."),
        new("30 August 1983", "Communication and Earth observation", "INSAT-1B launched",
            "The INSAT-1B communications satellite became operational and supported telecommunications, television broadcasting, and meteorology in India."),
        new("20 September 1993", "Launch vehicles", "PSLV's first flight",
            "The first Polar Satellite Launch Vehicle flight did not place its payloads in the intended orbit; lessons from the mission informed subsequent flights."),
        new("15 October 1994", "Launch vehicles", "First successful PSLV flight",
            "PSLV-D2 successfully placed the IRS-P2 remote-sensing satellite in orbit, beginning a long series of operational PSLV missions."),
        new("20 September 1999", "Communication and Earth observation", "Oceansat-1 launched",
            "India's first dedicated ocean-observation satellite, also known as IRS-P4, began collecting data on ocean colour and winds."),
        new("20 April 2001", "Launch vehicles", "GSLV's first development flight",
            "The Geosynchronous Satellite Launch Vehicle made its first development flight, expanding India's capability to launch heavier satellites."),
        new("22 October 2008", "Lunar and planetary missions", "Chandrayaan-1 launched",
            "India's first lunar mission entered lunar orbit and carried instruments from India and international partners. Its observations contributed evidence for water molecules on the Moon."),
        new("5 November 2013", "Lunar and planetary missions", "Mars Orbiter Mission launched",
            "ISRO launched its first interplanetary mission, also called Mangalyaan, to study Mars and demonstrate deep-space mission capabilities."),
        new("24 September 2014", "Lunar and planetary missions", "Mars Orbiter Mission enters Mars orbit",
            "The spacecraft successfully entered Mars orbit on its first attempt, making India the first country to do so on its maiden Mars mission."),
        new("28 September 2015", "Science missions", "AstroSat launched",
            "India's first dedicated multi-wavelength space observatory began observing celestial sources across several parts of the electromagnetic spectrum."),
        new("22 June 2016", "Launch vehicles", "Reusable launch vehicle technology demonstrator",
            "ISRO conducted the first experimental flight of its winged Reusable Launch Vehicle technology demonstrator."),
        new("5 June 2017", "Launch vehicles", "GSLV Mk III's first development flight",
            "The vehicle now known as LVM3 completed its first development flight, carrying the GSAT-19 communications satellite."),
        new("22 July 2019", "Lunar and planetary missions", "Chandrayaan-2 launched",
            "The mission carried an orbiter, lander, and rover to the Moon. The lander did not complete its planned landing, while the orbiter continued its science mission."),
        new("14 July 2023", "Lunar and planetary missions", "Chandrayaan-3 launched",
            "ISRO launched a mission designed to demonstrate a safe lunar landing and rover operations."),
        new("23 August 2023", "Lunar and planetary missions", "Chandrayaan-3 lands near the lunar south pole",
            "The Vikram lander made a successful soft landing, and Pragyan conducted surface operations. India became the fourth country to achieve a lunar soft landing."),
        new("2 September 2023", "Solar and space science", "Aditya-L1 launched",
            "India's first space-based solar observatory began its journey to study the Sun from a halo orbit around the Sun–Earth L1 point."),
        new("21 October 2023", "Human spaceflight", "Gaganyaan TV-D1 test",
            "The Test Vehicle Abort Mission demonstrated an in-flight crew escape system test as part of India's human spaceflight programme."),
        new("1 January 2024", "Science missions", "XPoSat launched",
            "India's X-ray Polarimeter Satellite began a mission to study the polarisation of X-rays from cosmic sources."),
        new("6 January 2024", "Solar and space science", "Aditya-L1 reaches halo orbit",
            "The spacecraft was inserted into its planned halo orbit around the Sun–Earth L1 point to observe the Sun."),
        new("16 August 2024", "Earth observation", "EOS-08 launched",
            "The Earth observation satellite was launched aboard the Small Satellite Launch Vehicle's third development flight."),
        new("30 December 2024", "Technology demonstrations", "SpaDeX launched",
            "ISRO launched the Space Docking Experiment mission to demonstrate rendezvous, docking, and undocking technologies in orbit."),
        new("16 January 2025", "Technology demonstrations", "SpaDeX achieves docking",
            "The two SpaDeX spacecraft successfully docked in orbit, demonstrating an important capability for future missions involving multiple spacecraft.")
    ];

    public ObservableCollection<SpaceMissionEvent> VisibleEvents { get; } = new();

    public SpaceMissionTimelineWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(940, 780));

        CategoryComboBox.ItemsSource = new[]
        {
            AllCategories,
            "Global space milestones",
            "ISRO foundations",
            "Launch vehicles",
            "Communication and Earth observation",
            "Earth observation",
            "Lunar and planetary missions",
            "Science missions",
            "Solar and space science",
            "Human spaceflight",
            "Technology demonstrations"
        };
        CategoryComboBox.SelectedIndex = 0;
        UpdateEvents();
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) =>
        UpdateEvents();

    private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        UpdateEvents();

    private void UpdateEvents()
    {
        var query = SearchBox.Text.Trim();
        var selectedCategory = CategoryComboBox.SelectedItem as string ?? AllCategories;
        var matches = Events.Where(spaceEvent =>
                (selectedCategory == AllCategories || spaceEvent.Category == selectedCategory) &&
                (query.Length == 0 ||
                 spaceEvent.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                 spaceEvent.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                 spaceEvent.DateLabel.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                 spaceEvent.Category.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        VisibleEvents.Clear();
        foreach (var spaceEvent in matches)
        {
            VisibleEvents.Add(spaceEvent);
        }

        ResultCountText.Text = matches.Length == 1 ? "1 event" : $"{matches.Length} events";
        EmptyStateText.Visibility = matches.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}

public sealed class SpaceMissionEvent
{
    public string DateLabel { get; set; }

    public string Category { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public SpaceMissionEvent(string dateLabel, string category, string title, string description)
    {
        DateLabel = dateLabel;
        Category = category;
        Title = title;
        Description = description;
    }
}
