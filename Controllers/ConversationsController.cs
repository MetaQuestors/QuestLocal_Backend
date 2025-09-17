using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using QuestLocalBackend.Data;
using QuestLocalBackend.Models;
using Message = QuestLocalBackend.Models.Message;

[Route("api/[controller]")]
[ApiController]
public class ConversationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ConversationsController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Conversation>>> GetConversations()
    {
        return await _context.Conversations.ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Conversation>> CreateConversation(Conversation conversation)
    {
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();
        return Ok(conversation);
    }

    [HttpGet("{id}/messages")]
    public async Task<ActionResult<IEnumerable<Message>>> GetMessages(int id)
    {
        return await _context.Messages.Where(m => m.ConversationId == id).ToListAsync();
    }

    [HttpPost("{id}/messages")]
    public async Task<ActionResult<Message>> PostMessage(int id, Message message)
    {
        message.ConversationId = id;
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return Ok(message);
    }
}
