using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.Graphics;

namespace Tools365.Education.IndianHistoryTimeline;

public sealed partial class IndianHistoryTimelineWindow : Window
{
    private const string AllPeriods = "All periods";

    private static readonly HistoryEvent[] HistoryEvents =
    [
        new("c. 2600–1900 BCE", "Ancient India", "Mature Harappan civilisation",
            "Planned urban settlements such as Harappa and Mohenjo-daro flourished across the Indus region, with drainage systems, craft production, and long-distance trade."),
        new("c. 1500–1000 BCE", "Ancient India", "Composition of early Vedic texts",
            "The Rigveda, among the oldest surviving Indo-Aryan texts, took shape over time in the northwestern subcontinent."),
        new("c. 600 BCE", "Ancient India", "Rise of the Mahajanapadas",
            "Sixteen major states and republics are described in early Buddhist and Jain sources, reflecting urban growth and new political centres."),
        new("6th century BCE", "Ancient India", "Buddhism and Jainism emerge",
            "Siddhartha Gautama and Vardhamana Mahavira taught paths that became the foundations of Buddhism and Jainism."),
        new("c. 322 BCE", "Ancient India", "Mauryan Empire founded",
            "Chandragupta Maurya established an empire that grew to encompass much of the Indian subcontinent."),
        new("c. 261 BCE", "Ancient India", "Kalinga War and Ashoka's edicts",
            "After the Kalinga campaign, Ashoka's inscriptions record his support for dhamma, public welfare, and restraint in warfare."),
        new("4th century CE", "Ancient India", "Gupta Empire expands",
            "The Gupta rulers built a major north Indian empire; this period saw developments in literature, mathematics, astronomy, and art."),
        new("c. 5th century CE", "Ancient India", "Nalanda becomes a centre of learning",
            "Nalanda grew into a renowned monastic and educational institution, attracting scholars from across Asia."),
        new("985–1014 CE", "Early medieval India", "Rajaraja Chola I reigns",
            "The Chola kingdom expanded under Rajaraja I, and the Brihadisvara Temple at Thanjavur was built during his reign."),
        new("1206 CE", "Medieval India", "Delhi Sultanate established",
            "Qutb al-Din Aibak's accession marks the beginning of the Delhi Sultanate, a succession of dynasties ruling from Delhi."),
        new("1336 CE", "Medieval India", "Vijayanagara founded",
            "The Vijayanagara kingdom emerged in the Deccan and became a major South Indian power and cultural centre."),
        new("1526 CE", "Medieval India", "Mughal Empire begins",
            "Babur's victory at the First Battle of Panipat established Mughal rule in northern India."),
        new("1556–1605 CE", "Medieval India", "Reign of Akbar",
            "Akbar consolidated Mughal rule and developed an imperial administration spanning much of northern and central India."),
        new("1674 CE", "Medieval India", "Shivaji crowned at Raigad",
            "Shivaji's coronation formalised the Maratha kingdom, which later became a major power across the subcontinent."),
        new("1757 CE", "Colonial India", "Battle of Plassey",
            "The East India Company's victory in Bengal marked a major expansion of its political power in India."),
        new("1857 CE", "Colonial India", "Uprising against Company rule",
            "A widespread uprising challenged East India Company authority; in 1858, governance was transferred to the British Crown."),
        new("1885 CE", "Colonial India", "Indian National Congress founded",
            "The Congress was established in Bombay and became a central organisation in India's political and independence movements."),
        new("1905 CE", "Colonial India", "Partition of Bengal",
            "The colonial partition of Bengal prompted widespread protest and helped galvanise the Swadeshi movement."),
        new("1919 CE", "Colonial India", "Jallianwala Bagh massacre",
            "British troops fired on a gathering in Amritsar, killing and injuring hundreds and intensifying opposition to colonial rule."),
        new("1930 CE", "Colonial India", "Salt March",
            "Mahatma Gandhi led a march to Dandi to challenge the British salt monopoly, helping launch a nationwide civil disobedience campaign."),
        new("1942 CE", "Colonial India", "Quit India Movement",
            "The Indian National Congress called for an end to British rule; the movement led to mass protests and arrests."),
        new("15 August 1947", "Modern India", "India becomes independent",
            "India achieved independence from British rule and was partitioned into India and Pakistan."),
        new("26 January 1950", "Modern India", "Constitution comes into force",
            "India became a republic when its Constitution took effect, establishing the framework of parliamentary democracy."),
        new("1960s", "Modern India", "Green Revolution",
            "New crop varieties, irrigation, and agricultural practices increased food-grain production, especially in northwestern India."),
        new("1991 CE", "Modern India", "Economic reforms begin",
            "India introduced major industrial, trade, and financial reforms that reshaped its economy."),
        new("23 August 2023", "Modern India", "Chandrayaan-3 lands on the Moon",
            "India's lunar mission achieved a soft landing near the Moon's south polar region, with the Vikram lander and Pragyan rover.")
    ];

    public ObservableCollection<HistoryEvent> VisibleEvents { get; } = new();

    public IndianHistoryTimelineWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(900, 760));

        PeriodComboBox.ItemsSource = new[]
        {
            AllPeriods,
            "Ancient India",
            "Early medieval India",
            "Medieval India",
            "Colonial India",
            "Modern India"
        };
        PeriodComboBox.SelectedIndex = 0;
        UpdateEvents();
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) =>
        UpdateEvents();

    private void PeriodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        UpdateEvents();

    private void UpdateEvents()
    {
        var query = SearchBox.Text.Trim();
        var selectedPeriod = PeriodComboBox.SelectedItem as string ?? AllPeriods;
        var matches = HistoryEvents.Where(historyEvent =>
                (selectedPeriod == AllPeriods || historyEvent.Period == selectedPeriod) &&
                (query.Length == 0 ||
                 historyEvent.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                 historyEvent.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                 historyEvent.DateLabel.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                 historyEvent.Period.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        VisibleEvents.Clear();
        foreach (var historyEvent in matches)
        {
            VisibleEvents.Add(historyEvent);
        }

        ResultCountText.Text = matches.Length == 1 ? "1 event" : $"{matches.Length} events";
        EmptyStateText.Visibility = matches.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}

public sealed class HistoryEvent
{
    public string DateLabel { get; set; }

    public string Period { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public HistoryEvent(string dateLabel, string period, string title, string description)
    {
        DateLabel = dateLabel;
        Period = period;
        Title = title;
        Description = description;
    }
}
