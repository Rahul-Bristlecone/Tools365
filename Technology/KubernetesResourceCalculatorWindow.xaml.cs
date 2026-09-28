using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Text.Json;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;

namespace Tools365.Technology;

public sealed partial class KubernetesResourceCalculatorWindow : Window
{
    private static readonly IReadOnlyDictionary<string, RecommendedInstance[]> InstancesByCategory = LoadInstancesByCategory();

    public KubernetesResourceCalculatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(980, 860));
        CalculateCapacity();
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        CalculateCapacity();
    }

    private void CopyYamlButton_Click(object sender, RoutedEventArgs e)
    {
        var package = new DataPackage();
        package.SetText(YamlOutputBox.Text);
        Clipboard.SetContent(package);
    }

    private void CalculateCapacity()
    {
        if (!TryReadInputs(out var input, out var error))
        {
            ValidationBar.Title = "Check the inputs";
            ValidationBar.Message = error;
            ValidationBar.IsOpen = true;
            return;
        }

        ValidationBar.IsOpen = false;

        var cpuUtilization = input.CpuUsage / input.CpuRequest * 100;
        var memoryUtilization = input.MemoryUsage / input.MemoryRequest * 100;
        var currentRpsPerPod = input.CurrentRps / input.CurrentPods;
        var recommendedPeakPods = Math.Max(1, Math.Ceiling(input.PeakRps / currentRpsPerPod));
        var targetPods = input.TargetPods ?? recommendedPeakPods;
        var decision = GetDecision(cpuUtilization, memoryUtilization);
        var suggestedCpuRequest = GetSuggestedCpuRequest(input, decision);
        var suggestedMemoryRequest = GetSuggestedMemoryRequest(input, decision);
        var suggestedCpuLimit = RoundUpTo(suggestedCpuRequest * 2, 50);
        var suggestedMemoryLimit = RoundUpTo(suggestedMemoryRequest * 2, 128);
        var cpuCapacity = targetPods * input.CpuRequest / 1000;
        var memoryCapacity = targetPods * input.MemoryRequest / 1024;
        var bufferedCpu = Math.Ceiling(cpuCapacity * 1.25);
        var bufferedMemory = Math.Ceiling(memoryCapacity * 1.25);
        var selectedInstance = SelectInstance(bufferedCpu, bufferedMemory);
        var nodesRequired = Math.Max(
            Math.Ceiling(bufferedCpu / selectedInstance.Vcpu),
            Math.Ceiling(bufferedMemory / selectedInstance.MemoryGiB));

        RecommendationTitleText.Text = decision.Title;
        AnalysisText.Text = $"CPU utilization: {cpuUtilization:0}%   Memory utilization: {memoryUtilization:0}%\n" +
            $"Current throughput: {currentRpsPerPod:0.##} RPS per pod.";
        ReasonText.Text = decision.Reason;
        ReplicaPlanText.Text = $"Current: {input.CurrentPods:0} pods\nPeak recommendation: {recommendedPeakPods:0} pods\n" +
            $"Capacity plan: {targetPods:0} pods";
        NodePlanText.Text = $"For {targetPods:0} pods: {cpuCapacity:0.##} vCPU, {memoryCapacity:0.##} GiB\n" +
            $"With 25% buffer: {bufferedCpu:0} vCPU, {bufferedMemory:0} GiB\n" +
            $"Recommended: {selectedInstance.Type} ({selectedInstance.Category})\n" +
            $"Nodes required: {nodesRequired:0}";
        YamlOutputBox.Text = $"resources:{Environment.NewLine}" +
            $"  requests:{Environment.NewLine}" +
            $"    cpu: {suggestedCpuRequest:0}m{Environment.NewLine}" +
            $"    memory: {FormatMemory(suggestedMemoryRequest)}{Environment.NewLine}" +
            $"  limits:{Environment.NewLine}" +
            $"    cpu: {suggestedCpuLimit:0}m{Environment.NewLine}" +
            $"    memory: {FormatMemory(suggestedMemoryLimit)}";
    }

    private bool TryReadInputs(out CapacityInput input, out string error)
    {
        input = new CapacityInput(CurrentPodsBox.Value, CpuRequestBox.Value, CpuUsageBox.Value, MemoryRequestBox.Value,
            MemoryUsageBox.Value, CurrentRpsBox.Value, PeakRpsBox.Value, TargetPodsBox.Value);

        if (double.IsNaN(input.CurrentPods) || double.IsNaN(input.CpuRequest) || double.IsNaN(input.CpuUsage) ||
            double.IsNaN(input.MemoryRequest) || double.IsNaN(input.MemoryUsage) || double.IsNaN(input.CurrentRps) || double.IsNaN(input.PeakRps))
        {
            error = "Current pods, CPU, memory, and RPS fields must contain numeric values.";
            return false;
        }

        if (input.CurrentPods <= 0 || input.CpuRequest <= 0 || input.CpuUsage < 0 || input.MemoryRequest <= 0 ||
            input.MemoryUsage < 0 || input.CurrentRps <= 0 || input.PeakRps <= 0 || (!double.IsNaN(input.TargetPodsValue) && input.TargetPodsValue <= 0))
        {
            error = "Use positive request, pod, and RPS values. Usage values cannot be negative.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static Decision GetDecision(double cpuUtilization, double memoryUtilization) =>
        (cpuUtilization, memoryUtilization) switch
        {
            (>= 80, >= 85) => new("Vertical + Horizontal Scaling Recommended", "Both CPU and memory are under pressure. Increase pod resources and add replicas for peak traffic."),
            (>= 80, < 75) => new("Horizontal Scaling Recommended", "The workload is CPU-bound and traffic distributes across replicas. Use HPA to add pods for peak demand."),
            (< 70, >= 85) => new("Vertical Memory Scaling Recommended", "Memory pressure is inside each container; more replicas alone will not resolve it. Increase the memory request and limit."),
            (< 50, < 50) => new("Downsize Resources", "CPU and memory requests are substantially above observed usage. Reduce requests to lower cluster cost."),
            _ => new("Monitor and Maintain", "Utilization is balanced. Keep autoscaling configured and revisit the plan as peak traffic changes.")
        };

    private static double GetSuggestedCpuRequest(CapacityInput input, Decision decision) => decision.Title switch
    {
        "Downsize Resources" => RoundUpTo(Math.Max(input.CpuUsage * 1.25, 50), 50),
        "Vertical + Horizontal Scaling Recommended" => RoundUpTo(Math.Max(input.CpuUsage * 1.25, input.CpuRequest), 50),
        _ => RoundUpTo(Math.Max(input.CpuRequest, input.CpuUsage * 1.2), 50)
    };

    private static double GetSuggestedMemoryRequest(CapacityInput input, Decision decision) => decision.Title switch
    {
        "Downsize Resources" => RoundUpTo(Math.Max(input.MemoryUsage * 1.25, 64), 64),
        "Vertical Memory Scaling Recommended" => RoundUpTo(Math.Max(input.MemoryRequest * 2, input.MemoryUsage * 1.25), 128),
        "Vertical + Horizontal Scaling Recommended" => RoundUpTo(Math.Max(input.MemoryRequest * 1.5, input.MemoryUsage * 1.25), 128),
        _ => RoundUpTo(Math.Max(input.MemoryRequest, input.MemoryUsage * 1.2), 64)
    };

    private static double RoundUpTo(double value, double increment) => Math.Ceiling(value / increment) * increment;

    private static RecommendedInstance SelectInstance(double bufferedCpu, double bufferedMemory)
    {
        var memoryPerVcpu = bufferedMemory / bufferedCpu;
        var preferredCategory = bufferedCpu <= 2 && bufferedMemory <= 4
            ? "burstable"
            : memoryPerVcpu >= 6
                ? "memory-optimized"
                : memoryPerVcpu <= 3
                    ? "compute-optimized"
                    : "general-purpose";
        var candidates = InstancesByCategory[preferredCategory];
        foreach (var candidate in candidates)
        {
            if (candidate.Vcpu >= bufferedCpu && candidate.MemoryGiB >= bufferedMemory)
            {
                return candidate;
            }
        }

        return candidates[^1];
    }

    private static IReadOnlyDictionary<string, RecommendedInstance[]> LoadInstancesByCategory()
    {
        var catalogPath = Path.Combine(AppContext.BaseDirectory, "Technology", "AwsInstanceCatalog.json");
        var catalogJson = File.ReadAllText(catalogPath);
        var families = JsonSerializer.Deserialize<List<InstanceFamily>>(catalogJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? [];

        return families.ToDictionary(
            family => family.Category,
            family => family.Instances
                .Select(instance => new RecommendedInstance(instance.Type, instance.Vcpu, instance.MemoryGiB, family.Category))
                .OrderBy(instance => instance.Vcpu)
                .ThenBy(instance => instance.MemoryGiB)
                .ToArray(),
            StringComparer.Ordinal);
    }

    private static string FormatMemory(double memoryMiB) => memoryMiB >= 1024
        ? $"{memoryMiB / 1024:0.##}Gi"
        : $"{memoryMiB:0}Mi";

    private sealed record CapacityInput(double CurrentPods, double CpuRequest, double CpuUsage, double MemoryRequest,
        double MemoryUsage, double CurrentRps, double PeakRps, double TargetPodsValue)
    {
        public double? TargetPods => double.IsNaN(TargetPodsValue) ? null : Math.Ceiling(TargetPodsValue);
    }

    private sealed record Decision(string Title, string Reason);

    private sealed record InstanceFamily(string Family, string Category, IReadOnlyList<InstanceType> Instances);

    private sealed record InstanceType(string Type, double Vcpu, double MemoryGiB);

    private sealed record RecommendedInstance(string Type, double Vcpu, double MemoryGiB, string Category);
}
