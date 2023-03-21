using Graduate_Project_BackEnd.Models;
using Graduate_Project_BackEnd.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PusherServer;

namespace Graduate_Project_BackEnd.Controllers
{
  [Route("[controller]/[action]")]
  [ApiController]
  public class ChatController : Controller
  {
    DBCONTEXT DB;
    public ChatController(DBCONTEXT dB)
    {
      DB = dB;
    }
    public IActionResult Add([FromBody] ChatVM chat)
    {
      bool vf_dr = DB.Teams.Any(t => t.Id == chat.TeamId && t.ProfID == chat.SenderId);
      bool vf_st = DB.Courses_Students.Any(cs => cs.TeamID == chat.TeamId && cs.StudentID == chat.SenderId);
      if (vf_dr || vf_st)
      {
        ChatModel newchat = new()
        {
          Message = chat.Message,
          SenderId = chat.SenderId,
          TeamID = chat.TeamId,
          Role = chat.Role,
        };
        DB.Chat.Add(newchat);
        DB.SaveChanges();
        return Json(new { state = true, msg = "Success" });
      }
      return Json(new { state = false, msg = "You Not Accessed!" });
    }
    public IActionResult Display([FromForm] int team_id)
    {
      List<ChatModel> chats = DB.Chat.Where(c => c.TeamID == team_id).ToList();
      return Json(new { state = true, msg = "Success", data = chats });
    }
    [HttpGet]
    public async void Pusher_notifiy()
    {
      var pusher = new Pusher(
      "1555576",
      "6b02096c815db94e569b",
      "33153ac9dce2b548644e",
      new PusherOptions
      {
        Cluster = "eu",
        Encrypted = true,
      });
      await pusher.TriggerAsync("chat-channel", "chat-ev", new { });
    }
  }
}
