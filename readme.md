# GTFAQ Workshop - Building a .NET bot

This tutorial will show you how to setup a FAQ bot using the Microsoft Bot Framework and LUIS.AI as a natural language processing service.

## Setup
### Prerequisites
1. Install Visual Studio 2015 - http://www.visualstudio.com
2. Download the Bot Framework VS Template at http://aka.ms/bf-bc-vstemplate
3. Save the ZIP file to your template directory which by default is:
```
%USERPROFILE%\Documents\Visual Studio 2015\Templates\ProjectTemplates\Visual C#\
```
4. Download the Bot Channel Emulator for testing at https://aka.ms/bf-bc-emulator (Windows only)
5. Create a new Visual Studio project using the newly installed template.

![New Project](https://docs.botframework.com/en-us/images/connector/connector-getstarted-create-project.png)

### What does the template give me?
The bot framework template at its core is essentially a web service which accepts and sends HTTP POST requests.
The VS template builds on top of a standard web service, providing a skeleton/base point for building your .NET bot.

The main file to focus on when first starting on a bot is `MessagesController.cs`.
This is the endpoint where messages sent from the connector to your bot's web service will be received in the form of HTTP POST requests.

By default, the `MessagesController` class already has some basic code to echo back the message sent by the user along with the length of the user's message.

```cs
ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
// calculate something for us to return
int length = (activity.Text ?? string.Empty).Length;

// return our reply to the user
Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
await connector.Conversations.ReplyToActivityAsync(reply);
```

### How do I test my bot?

The bot framework template at its core is essentially a web service which accepts and sends HTTP POST requests. By running it in Visual Studio, it will open a localhost instance of the bot which you can communicate with using the emulator.

During the development of the bot, you can test and debug the bot using the Bot Channel emulator. By emptying the app ID and password at the top of the emulator and setting the right port, you can then send messages to the bot running on localhost.

![Bot Emulator localhost setup](http://i.imgur.com/Eq6uhZR.png)

Ensure that you can run the project as well as use the emulator before continuing with this tutorial.

## Configuring LUIS
LUIS is required in order for your bot to understand natural language from users.

1. Go to https://luis.ai
2. Sign in with a Microsoft account (Hotmail, Outlook etc.).
3. Create a new app.
4. An intent named "None" has been pre-created which LUIS will default to if it does not find any intents. Add 4 intents named:
  * Greeting - Example utterance: "Hello"
  * WhatIs - Example utterance: "What is GovTech?"
  * Chairman - Example utterance: "Who is the chairman of GovTech?"
  * News - Example utterance: "Where can I read up on tech news?"
5. From there, you can add more utterances to train the intents.
6. Click publish on the top left and take note of your LUIS ID and key in the endpoint URL.

## Implementing LUIS into the bot
1. Create a new dialog class named MainDialog. This extends LuisDialog which is one of the built-in dialogs that will call the LUIS.AI service. Ensure that the LUIS ID and key are pasted into the property correctly.
  ```cs
  using Microsoft.Bot.Builder.Dialogs;
  using Microsoft.Bot.Builder.Luis;
  using Microsoft.Bot.Builder.Luis.Models;
  using Microsoft.Bot.Connector;

  [LuisModel("Enter LUIS ID here","Enter LUIS key here")]
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
          await context.PostAsync("The Government Technology Agency of Singapore (GovTech) is a new statutory board formed in October 2016 after the restructuring of the Infocomm Development Authority (IDA) and the Media Development Authority (MDA). At GovTech, we transform the delivery of Government digital services by taking an outside-in view -- this means seeing issues from your perspective and putting citizens and businesses at the heart of everything we do.");
          context.Wait(MessageReceived);
      }

      [LuisIntent("Chairman")]
      public async Task Chairman(IDialogContext context, LuisResult result)
      {
          await context.PostAsync("The chairman of GovTech is Mr Ng Chee Khern.");
          context.Wait(MessageReceived);
      }

      [LuisIntent("News")]
      public async Task News(IDialogContext context, LuisResult result)
      {
          await context.PostAsync("You can find TechNews by GovTech at https://www.tech.gov.sg/en/TechNews");
          context.Wait(MessageReceived);
      }
  }
  ```
  This dialog in its current state will listen for the "Greeting", "WhatIs", "Chairman" and "News" intents and if not found, it will default to the "" intent. Based on the intent detected by the LUIS service, the corresponding method will be called.

2. In the `MessagesController` class, use a lambda expression to hand-off the conversation to the newly created LUIS dialog.

  Replace the code which echos:
  ```cs
  ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
  // calculate something for us to return
  int length = (activity.Text ?? string.Empty).Length;

  // return our reply to the user
  Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
  await connector.Conversations.ReplyToActivityAsync(reply);
  ```

  With:

  ```cs
  await Conversation.SendAsync(activity, () => new MainDialog());
  ```

3. Once done, try to launch your bot again and test if it works as intended using the emulator. If the intent is detected wrongly, go back to the LUIS.AI web portal to set the intent correctly and re-train and re-publish your bot to make it smarter for the next round.

## Adding rich content
So far, the bot will only respond using plaintext. However, the bot supports richer content like cards and images which can be added as attachments.

NOTE: Cards will not work as intended in the embedded web chat, but will work in the emulator and Skype.

In this example, we will be adding a card to the Chairman message to make the messages more interesting. In the method for the Chairman intent, we can add the following code to initialize a message with attachments and add a card to the attachment list.

```cs
[LuisIntent("Chairman")]
public async Task Chairman(IDialogContext context, LuisResult result)
{
    //Generate a message
    var message = context.MakeMessage();
    message.Attachments = new List<Attachment>();

    //Initialize images
    List<CardImage>imageList = new List<CardImage>();
    imageList.Add(new CardImage(url: "https://www.tech.gov.sg/-/media/GovTech/About-us/Board-Of-Directors/Mr-Ng-Chee-Khern.jpg"));

    //Initialize buttons
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
```
Try running the bot again and ask it about the chairman in the emulator. You should be able to see the card being displayed.

## Pulling from APIs
The bot at its core is a .NET web service. As such, it is able to do anything that a standard web service can do, including pulling from other APIs or database web services.

For the purposes of this tutorial, I will be serving a mock JSON from my web service which contains an array of news articles from GovTech. In the future, this could be connected to any proper REST API. The JSON deserialization in this section will be done using the JSON.NET library from Newtonsoft.

In this example, when the user asks for news, we will retrieve news from the web service and then display them as a card carousel.

Create the classes below to retrieve JSON from the mock web service and parse it.

**Model Class**
```cs
public class NewsArticle
{
    public string Title { get; set; }
    public string Date { get; set; }
    public string ImageURL { get; set; }
    public string ArticleURL { get; set; }
}
```

**Data Retrieval Class**
```cs
using Newtonsoft.Json;

public class NewsArticleAccess
{
    public static async Task<List<NewsArticle>> GetNewsArticlesAsync()
    {
        var result = "";

        //Create new list
        List<NewsArticle> articleList = new List<NewsArticle>();

        try
        {
            //Send request and get the JSON
            HttpClient httpClient = new HttpClient();
            var httpResponse = await httpClient.GetAsync("http://govtechdemo.azurewebsites.net/service/newsretrieve");
            httpResponse.EnsureSuccessStatusCode();
            result = await httpResponse.Content.ReadAsStringAsync();
            Debug.WriteLine(result);

            //Deserialize the JSON array into a list
            articleList = JsonConvert.DeserializeObject<List<NewsArticle>>(result);
        }
        catch (Exception ex)
        {
            result = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            Debug.WriteLine(result);
        }

        return articleList;
    }
}
```

Using the data which is retrieved into a list, we can then iterate through the list and then add cards to the message for each entity which will display as a carousel. We can implement this within the "News" intent in the ```MainDialog``` class.

NOTE: Carousels are currently officially supported in Skype and can display a maximum of 5 cards.

```cs
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
```

Try launching the bot again and ask for news. You should be able to see a carousel of cards containing news as well as a link to read more articles.

## Go to market!
In order for your bot to work on the many available channels like Skype or Facebook, or any environment outside of the emulator, it has to be registered on the Bot Framework portal as well as deployed to a proper server where an endpoint can be exposed.

1. To register the bot, go to the bot framework portal at https://dev.botframework.com.
2. Sign in with your Microsoft account and click Register a bot.
3. Fill up the form as appropriate, taking note of your Microsoft App ID, Password and bot handle.

Go back to your bot project on Visual Studio and open Web.Config. Replace the app settings as appropriate.
```cs
<add key="BotId" value="Bot Handle here" />
<add key="MicrosoftAppId" value="Microsoft App ID here" />
<add key="MicrosoftAppPassword" value="Microsoft Password here" />
```

From there, the project is a web app that can be deployed to any cloud or on-premise server of your choice that supports .NET.
(Even Google Cloud Platform, if that's your thing.)

Visual Studio 2015 comes with Azure App Services (PaaS) integration. This allows you to right click the solution and click publish. If you have an Azure subscription, you will be able to create a Web App resource on Azure and deploy directly to it easily.

## Try it out
http://govtechdemo.azurewebsites.net/

## Further learning
There are many other bot framework features to look into, like prompts, authentication, dialog chaining and FormFlows.

| Resource | Link |
| --- | --- |
| Official documentation | https://docs.botframework.com/en-us/ |
| Resource collection | https://aka.ms/botresources |
