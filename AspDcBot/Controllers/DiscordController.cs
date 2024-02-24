using AspDcBot.Models;
using AspDcBot.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspDcBot.Controllers;
[Route("api/[controller]")]
[ApiController]
public class DiscordController(DiscordBotService discordBotService) : ControllerBase
{
    [HttpGet("start")]
    public async Task Start()
    {
        await discordBotService.StartAsync();
    }

    [HttpGet("stop")]
    public async Task Stop()
    {
        await discordBotService.StopAsync();
    }

    [HttpGet("ping/{message}")]
    public async Task Ping([FromRoute] string message = "ping")
    {
        await discordBotService.PingAsync(HttpContext.RequestAborted, message);
    }

    [HttpGet("guildsAndChannels")]
    public List<Guild> GetGuildsAndChannels()
    {
        return discordBotService.GetGuildsAndChannels();
    }

    [HttpGet("forcestop")]
    public async Task ForceStopAsync()
    {
        await discordBotService.ForceStopAsync();
    }

    [HttpGet("deleteglobalcommands")]
    public async Task DeleteGlobalCommands()
    {
        await discordBotService.ClearGlobalCommands();
    }
}
