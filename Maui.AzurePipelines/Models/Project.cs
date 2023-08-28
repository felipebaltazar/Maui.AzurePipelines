using Microsoft.Maui.Controls;
using PipelineApproval.Abstractions;
using PipelineApproval.Infrastructure.Commands;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace PipelineApproval.Models;

public class Project
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string url { get; set; }
    public string state { get; set; }

    public string defaultTeamImageUrl { get; set; }

    public string TeamImageFile { get; set; }

    [JsonIgnore]
    public IAsyncCommand NavigateToProjectCommand { get; set; }

    public Color GenerateColor()
    {
        using (MD5 md5Hash = MD5.Create())
        {
            var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(name));
            var colorHex = BitConverter.ToString(data).Replace("-", string.Empty).Substring(0, 6);
            return Color.FromArgb("#" + colorHex);
        }
    }
}
