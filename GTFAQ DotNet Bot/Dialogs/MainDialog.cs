using GTFAQ_DotNet_Bot.Data;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GTFAQ_DotNet_Bot.Dialogs
{
    [LuisModel("LUIS ID here", "LUIS Key here")]
    [Serializable]
    public class MainDialog : LuisDialog <Object>
    {
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I can't understand your message at the moment, sorry!");
            context.Wait(MessageReceived);
        } 

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hello! I'm Jamie :) I answer questions about GovTech.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("WhatIs")]
        public async Task WhatIs(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("The Government Technology Agency of Singapore (GovTech) is a new statutory board formed in October 2016 after the restructuring of the Infocomm Development Authority (IDA) and the Media Development Authority (MDA). At GovTech, we transform the delivery of Government digital services by taking an outside -in view -- this means seeing issues from your perspective and putting citizens and businesses at the heart of everything we do.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Role")]
        public async Task Role(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("GovTech works with public agencies to develop and deliver secure digital services and applied technology to individuals and businesses in Singapore. GovTech builds key platforms and solutions needed to support Singapore as a Smart Nation. As a leading centre for information communications technology (ICT) and related engineering such as the Internet of Things, GovTech also enhances the capabilities of the Singapore Government in these domains.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Chairman")]
        public async Task Chairman(IDialogContext context, LuisResult result)
        {
            //Generate a message
            var message = context.MakeMessage();
            message.Attachments = new List<Attachment>();

            //Generate images
            List<CardImage>imageList = new List<CardImage>();
            imageList.Add(new CardImage(url: "https://www.tech.gov.sg/-/media/GovTech/About-us/Board-Of-Directors/Mr-Ng-Chee-Khern.jpg"));

            //Generate buttons
            List<CardAction> buttonList = new List<CardAction>();
            buttonList.Add(new CardAction()
            {
                Value = "https://www.tech.gov.sg/en/About-Us/Organisation-Team/Board-of-Directors",
                Type = "openUrl",
                Title = "See all directors"
            });       

            //Generate card
            var card = new HeroCard()
            {
                Title = "Mr Ng Chee Khern",
                Text = "Chairman of GovTech",
                Images = imageList,
                Buttons = buttonList
            };

            //Add card to message
            message.Attachments.Add(card.ToAttachment());

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("News")]
        public async Task News(IDialogContext context, LuisResult result)
        {
            //Make card carousel
            var message = context.MakeMessage();
            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            message.Attachments = new List<Attachment>();

            //Retrieve news from the web service
            List<NewsArticle>articleList = await NewsArticleAccess.GetNewsArticlesAsync();

            //Make cards, limit to 5 cause carousel only supports 5
            for (int i = 0; i < 5 && i < articleList.Count; i++)
            {
                //Assign current iteration to variable
                var article = articleList[i];

                //Generate buttons
                List<CardAction> buttonList = new List<CardAction>();
                buttonList.Add(new CardAction
                {
                    Value = $"{article.ArticleURL}",
                    Type = "openUrl",
                    Title = "More details"
                });

                //Generate images
                List<CardImage> imageList = new List<CardImage>();
                imageList.Add(new CardImage(article.ImageURL));

                //Generate card
                HeroCard card = new HeroCard()
                {
                    Title = article.Title,
                    Subtitle = article.Date,
                    Images = imageList,
                    Buttons = buttonList
                };

                //Add card to message
                message.Attachments.Add(card.ToAttachment());
            }

            //Send messages
            await context.PostAsync("Here are the top news:");
            await context.PostAsync(message);
            await context.PostAsync("You can see more news at https://www.tech.gov.sg/en/TechNews.");
     
            context.Wait(MessageReceived);
        }

        [LuisIntent("Job")]
        public async Task Job(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("We have several jobs available. You can see all the job openings at https://www.tech.gov.sg/sub/careers.");
            context.Wait(MessageReceived);
        }

    }
}