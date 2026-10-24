namespace Tools365.Family.VaccinationReminder;

public sealed class VaccinationReminderState
{
    public DateOnly? ChildDateOfBirth { get; set; }

    public bool IsJeEndemicArea { get; set; }

    public Dictionary<string, DateOnly> AdministeredDates { get; set; } = new();
}

public sealed record VaccinationDose(
    string Id,
    string Vaccine,
    string Timing,
    int StartDays = 0,
    int StartMonths = 0,
    int EndDays = 0,
    int EndMonths = 0,
    bool JeEndemicOnly = false)
{
    public DateOnly GetStartDate(DateOnly birthDate) => birthDate.AddDays(StartDays).AddMonths(StartMonths);

    public DateOnly GetEndDate(DateOnly birthDate) => birthDate.AddDays(EndDays).AddMonths(EndMonths);
}

public static class VaccinationSchedule
{
    private static readonly IReadOnlyList<VaccinationDose> Doses =
    [
        new("bcg", "BCG", "At birth", EndDays: 1),
        new("opv-0", "OPV-0", "At birth", EndDays: 1),
        new("hepb-birth", "Hepatitis B birth dose", "At birth (within 24 hours)", EndDays: 1),
        new("opv-1", "OPV 1", "6 weeks", StartDays: 42),
        new("pentavalent-1", "Pentavalent 1", "6 weeks", StartDays: 42),
        new("ipv-1", "fIPV 1", "6 weeks", StartDays: 42),
        new("rotavirus-1", "Rotavirus 1", "6 weeks", StartDays: 42),
        new("pcv-1", "PCV 1", "6 weeks", StartDays: 42),
        new("opv-2", "OPV 2", "10 weeks", StartDays: 70),
        new("pentavalent-2", "Pentavalent 2", "10 weeks", StartDays: 70),
        new("rotavirus-2", "Rotavirus 2", "10 weeks", StartDays: 70),
        new("opv-3", "OPV 3", "14 weeks", StartDays: 98),
        new("pentavalent-3", "Pentavalent 3", "14 weeks", StartDays: 98),
        new("rotavirus-3", "Rotavirus 3", "14 weeks", StartDays: 98),
        new("ipv-2", "fIPV 2", "14 weeks", StartDays: 98),
        new("pcv-2", "PCV 2", "14 weeks", StartDays: 98),
        new("mr-1", "MR 1", "9–12 months", StartMonths: 9, EndMonths: 12),
        new("pcv-booster", "PCV booster", "9–12 months", StartMonths: 9, EndMonths: 12),
        new("ipv-3", "fIPV 3", "9–12 months", StartMonths: 9, EndMonths: 12),
        new("je-1", "JE 1", "9–12 months (endemic districts only)", StartMonths: 9, EndMonths: 12, JeEndemicOnly: true),
        new("mr-2", "MR 2", "16–24 months", StartMonths: 16, EndMonths: 24),
        new("dpt-booster-1", "DPT booster 1", "16–24 months", StartMonths: 16, EndMonths: 24),
        new("opv-booster", "OPV booster", "16–24 months", StartMonths: 16, EndMonths: 24),
        new("je-2", "JE 2", "16–24 months (endemic districts only)", StartMonths: 16, EndMonths: 24, JeEndemicOnly: true),
        new("dpt-booster-2", "DPT booster 2", "5–6 years", StartMonths: 60, EndMonths: 72),
        new("td-10", "Td", "10 years", StartMonths: 120),
        new("td-16", "Td", "16 years", StartMonths: 192)
    ];

    public static IEnumerable<VaccinationDose> GetDoses(bool includeJeDoses) =>
        Doses.Where(dose => includeJeDoses || !dose.JeEndemicOnly);
}

public sealed class VaccinationDoseViewModel
{
    public VaccinationDoseViewModel(
        VaccinationDose dose,
        DateOnly dueDate,
        DateOnly dueWindowEnd,
        DateOnly? administeredDate)
    {
        Id = dose.Id;
        Vaccine = dose.Vaccine;
        Timing = dose.Timing;
        DueDate = dueDate;
        DueWindowEnd = dueWindowEnd;
        AdministeredDate = administeredDate is null
            ? null
            : new DateTimeOffset(administeredDate.Value.ToDateTime(TimeOnly.MinValue));
        Status = GetStatus(dueDate, dueWindowEnd, administeredDate, DateOnly.FromDateTime(DateTime.Today));
    }

    public string Id { get; }

    public string Vaccine { get; }

    public string Timing { get; }

    public DateOnly DueDate { get; }

    public DateOnly DueWindowEnd { get; }

    public DateTimeOffset? AdministeredDate { get; }

    public DateTimeOffset MaxAdministeredDate => DateTimeOffset.Now.Date;

    public string DueDateText => DueWindowEnd == DueDate
        ? $"Due {DueDate:dd MMM yyyy}"
        : $"Due {DueDate:dd MMM yyyy} – {DueWindowEnd:dd MMM yyyy}";

    public string Status { get; }

    public bool HasAdministeredDate => AdministeredDate.HasValue;

    private static string GetStatus(
        DateOnly dueDate,
        DateOnly dueWindowEnd,
        DateOnly? administeredDate,
        DateOnly today)
    {
        if (administeredDate is not null)
        {
            return $"Recorded as given on {administeredDate.Value:dd MMM yyyy}";
        }

        if (today < dueDate)
        {
            return "Upcoming";
        }

        return today <= dueWindowEnd ? "Due" : "Overdue — ask a health worker about catch-up";
    }
}
