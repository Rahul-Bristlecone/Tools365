using Tools365.Pages;

namespace Tools365.Technology;

public static class TechnologyToolCatalog
{
    public static IReadOnlyList<ToolCard> GetCards()
    {
        return new[]
        {
            new ToolCard("YAML / JSON converter", "Convert between YAML and JSON formats.", isConverter: true),
            new ToolCard("Cron expression generator", "Build and understand cron scheduling expressions."),
            new ToolCard("DB connection string generator", "Build database connection strings from common settings."),
            new ToolCard("API response time calculator", "Analyze response-time samples with average, median, and percentile metrics."),
            new ToolCard("CURL Generator", "Build a copy-ready cURL command from a request URL, headers, and body."),
            new ToolCard("TLS certificate generator", "Create a self-signed TLS certificate and password-protected private key."),
            new ToolCard("JWT Decoder", "Inspect a JWT header and payload locally without verifying its signature."),
            new ToolCard("SSL Certificate Checker", "Inspect the TLS certificate and protocol presented by a host."),
            new ToolCard("CIDR calculator", "Calculate IPv4 network details, address ranges, and usable host counts from CIDR notation."),
            new ToolCard(
                "Kubernetes Resource Calculator",
                "Estimate pod resources, cluster capacity, scaling, and EKS cost.",
                isKubernetesCalculator: true,
                subCards: new[]
                {
                    new ToolSubCard("Pod Resource Recommendation", "Estimate CPU and memory requests and limits from average and peak usage."),
                    new ToolSubCard("Cluster Capacity Calculator", "Calculate total CPU and memory for services, pod requests, and replicas."),
                    new ToolSubCard("HPA Calculator", "Estimate desired replicas from current CPU usage and the target utilization."),
                    new ToolSubCard("EKS Cost Calculator", "Estimate monthly infrastructure cost from vCPU, memory, runtime, and region."),
                    new ToolSubCard("Kubernetes Deployment Planner", "Plan replicas, requests, limits, HPA settings, and total cluster capacity from workload demand.")
                })
        };
    }
}
