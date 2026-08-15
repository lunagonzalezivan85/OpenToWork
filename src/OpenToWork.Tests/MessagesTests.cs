using System.Net;
using System.Net.Http.Json;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Tests;

/// <summary>
/// Pruebas de mensajes: conversaciones, mensajes de una conversación, enviar, marcar como leído.
/// </summary>
public class MessagesTests : BaseTest
{
    [Fact]
    public async Task GetConversations_ConTokenValido_RetornaLista()
    {
        await AuthenticateAsync();

        var response = await Client.GetAsync("api/messages/conversations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var conversations = await response.Content.ReadFromJsonAsync<List<ConversationDto>>();
        Assert.NotNull(conversations);
        Assert.NotEmpty(conversations);
    }

    [Fact]
    public async Task GetConversations_SinToken_RetornaUnauthorized()
    {
        var response = await Client.GetAsync("api/messages/conversations");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetConversations_RetornaDatosConEstructuraCorrecta()
    {
        await AuthenticateAsync();

        var response = await Client.GetAsync("api/messages/conversations");
        var conversations = await response.Content.ReadFromJsonAsync<List<ConversationDto>>();

        Assert.NotNull(conversations);
        Assert.NotEmpty(conversations);

        var first = conversations!.First();
        Assert.False(string.IsNullOrEmpty(first.ParticipantName));
        Assert.False(string.IsNullOrEmpty(first.ParticipantAvatar));
        Assert.False(string.IsNullOrEmpty(first.LastMessage));
    }

    [Fact]
    public async Task GetMessages_ConConversationIdValido_RetornaMensajes()
    {
        await AuthenticateAsync();

        var convResponse = await Client.GetAsync("api/messages/conversations");
        var conversations = await convResponse.Content.ReadFromJsonAsync<List<ConversationDto>>();

        if (conversations?.Any() == true)
        {
            var convId = conversations.First().Id;
            var response = await Client.GetAsync($"api/messages/{convId}/messages");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var messages = await response.Content.ReadFromJsonAsync<List<MessageDto>>();
            Assert.NotNull(messages);
            Assert.NotEmpty(messages);

            var first = messages!.First();
            Assert.False(string.IsNullOrEmpty(first.Content));
            Assert.False(string.IsNullOrEmpty(first.SenderName));
        }
    }

    [Fact]
    public async Task GetMessages_ConIdInexistente_RetornaListaVacia()
    {
        await AuthenticateAsync();

        var fakeId = Guid.NewGuid();
        var response = await Client.GetAsync($"api/messages/{fakeId}/messages");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var messages = await response.Content.ReadFromJsonAsync<List<MessageDto>>();
        Assert.NotNull(messages);
    }

    [Fact]
    public async Task SendMessage_ConDatosValidos_RetornaMensajeCreado()
    {
        await AuthenticateAsync();

        var convResponse = await Client.GetAsync("api/messages/conversations");
        var conversations = await convResponse.Content.ReadFromJsonAsync<List<ConversationDto>>();

        if (conversations?.Any() == true)
        {
            var convId = conversations.First().Id;
            var dto = new SendMessageDto
            {
                ConversationId = convId,
                Content = "Mensaje de prueba automatizada " + DateTime.Now.Ticks
            };

            var response = await Client.PostAsJsonAsync("api/messages/send", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var msg = await response.Content.ReadFromJsonAsync<MessageDto>();
            Assert.NotNull(msg);
            Assert.True(msg!.IsMine);
            Assert.False(string.IsNullOrEmpty(msg.Content));
            Assert.Contains("Mensaje de prueba", msg.Content);
        }
    }

    [Fact]
    public async Task SendMessage_SinToken_RetornaUnauthorized()
    {
        var dto = new SendMessageDto
        {
            ConversationId = Guid.NewGuid(),
            Content = "Test"
        };

        var response = await Client.PostAsJsonAsync("api/messages/send", dto);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_ConContenidoVacio_RetornaBadRequest()
    {
        await AuthenticateAsync();

        var dto = new SendMessageDto
        {
            ConversationId = Guid.NewGuid(),
            Content = ""
        };

        var response = await Client.PostAsJsonAsync("api/messages/send", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MarkAsRead_ConConversationIdValido_RetornaOk()
    {
        await AuthenticateAsync();

        var convResponse = await Client.GetAsync("api/messages/conversations");
        var conversations = await convResponse.Content.ReadFromJsonAsync<List<ConversationDto>>();

        if (conversations?.Any() == true)
        {
            var convId = conversations.First().Id;
            var response = await Client.PutAsync($"api/messages/{convId}/read", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task MarkAsRead_SinToken_RetornaUnauthorized()
    {
        var response = await Client.PutAsync($"api/messages/{Guid.NewGuid()}/read", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
