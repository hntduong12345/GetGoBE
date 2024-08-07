﻿using GetGo.Domain.Models;
using GetGo.Domain.Payload.Request.Message;
using GetGo.Domain.Payload.Request.Route;
using GetGo.Domain.Payload.Response.Messages;
using GetGo_BE.Constants;
using GetGo_BE.Enums.Message;
using GetGo_BE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.Text;

namespace GetGo_BE.Controllers
{
    [Authorize]
    [ApiController]
    public class MessageController : BaseController<MessageController>
    {
        private readonly IMessageService _messageService;
        private readonly IMapService _mapService;
        private readonly IAIMessageHistoryService _aiMessageHistoryService;
        private readonly IUserService _userService;

        public MessageController(ILogger<MessageController> logger, IMessageService messageService, IMapService mapService,
            IAIMessageHistoryService aiMessageHistoryService, IUserService userService) : base(logger)
        {
            _messageService = messageService;
            _mapService = mapService;
            _aiMessageHistoryService = aiMessageHistoryService;
            _userService = userService;
        }

        [HttpPost(ApiEndPointConstant.Message.MessagesEndpoint)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Create message")]
        public async Task<IActionResult> CreateMessage([FromBody] CreateMessageRequest request)
        {
            await _messageService.CreateMessage(request);
            return Ok("Action success");
        }

        [HttpDelete(ApiEndPointConstant.Message.MessageEndpoint)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Delete message")]
        public async Task<IActionResult> DeleteMessage(string id)
        {
            await _messageService.DeleteMessage(id);
            return Ok("Action success");
        }

        [HttpPost(ApiEndPointConstant.Message.DialogMessagesEndpoint)]
        [ProducesResponseType(typeof(List<Message>), StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Get message history list")]
        public async Task<IActionResult> GetDialogMessage([FromBody] GetDialogMessageRequest request)
        {
            var result = await _messageService.GetUserMessageHistory(request);
            return Ok(result);
        }

        [HttpPost(ApiEndPointConstant.Message.AIChatMessageEndpoint)]
        [ProducesResponseType(typeof(LocationSuggestionMessageResponse), StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Get location suggestion from ai")]
        public async Task<IActionResult> AIChat(string question, string userId)
        {
            try
            {
                //Get user subscription
                string userSubscription = await _userService.GetUserSubscription(userId);

                //Create MessageHistory for storing
                AIMessageHistory message = new AIMessageHistory(userId, AIChatEnum.CHATAGENT.ToString(), DateTime.Now, question);

                var result = new LocationSuggestionMessageResponse();
                using (var httpClient = new HttpClient())
                {
                    //Create Request for AI chat
                    AIChatRequest request = await _aiMessageHistoryService.GetAIChatHistory(new GetDialogMessageRequest()
                    {
                        User1 = userId,
                        User2 = AIChatEnum.CHATAGENT.ToString()
                    });
                    request.question = question.ToLower();

                    //Convert content into content in URL Body
                    var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                    //Add API-KEY of AI
                    httpClient.DefaultRequestHeaders.Add("X-API-Key", "ZjFkOTk2MDQtOTUwMi00OTk3LWE4MWEtODc2N2E2MTc1YjM5");
                    
                    //Call AI API for usage
                    using (var response = await httpClient.PostAsync($"https://pphuc25-getgo-ai.hf.space/agents/chat-agent?user_status={userSubscription}", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            result = Newtonsoft.Json.JsonConvert.DeserializeObject<LocationSuggestionMessageResponse>(responseContent);
                        }
                    }
                }

                //Add AI message to message history
                if (result != null)
                {
                    message.Answer = result;
                }

                //Add new Map
                if (result.locations_message != null)
                {
                    if (result.locations_message.locations != null)
                    {
                        await _mapService.CreateMap(new CreateMapRequest(userId, result.locations_message.locations));
                    }
                }

                //Create message history
                await _aiMessageHistoryService.CreateAIMessageHistory(message);

                return Ok(result);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet(ApiEndPointConstant.Message.AIChatMessageHistoryEndpoint)]
        [ProducesResponseType(typeof(List<HistoryRequest>), StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Get ai message history list by userId")]
        public async Task<IActionResult> GetAIChatHistory(string userId)
        {
            var result = await _aiMessageHistoryService.GetAIChatHistory(userId);
            return Ok(result);
        }
    }
}
