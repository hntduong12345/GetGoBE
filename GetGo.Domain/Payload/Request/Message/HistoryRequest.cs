﻿using GetGo.Domain.Payload.Response.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetGo.Domain.Payload.Request.Message
{
    public class HistoryRequest
    {
        public string? question {  get; set; }
        public LocationSuggestionMessageResponse? answer { get; set; }

        public HistoryRequest(string? question, LocationSuggestionMessageResponse? answer)
        {
            this.question = question;
            this.answer = answer;
        }
    }
}
