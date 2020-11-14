const Discord = require('discord.js');
const CONFIG = require('./config.json');
const bot = new Discord.Client();


bot.login(CONFIG.Token);
const PREFIX = CONFIG.Prefix;
const rThreshhold = CONFIG.rThreshhold;
const lThreshhold = CONFIG.lThreshhold;
const lengthThreshhold = CONFIG.lengthThreshhold;
const triggerChance = CONFIG.triggerChance;


bot.once('ready', () => {
    this.bot = bot
    bot.user.setActivity("~ U w U ~")
    console.log("BeeP BooP Bot Online Now")//successful startup log
});

bot.on('message', message =>{
    try{
        if (!checkWhetherToRespond(message)) return;
        var messageCheck = message.content.toLowerCase();
        if(altTriggers(message,messageCheck)) return;;
        if (willTrigger(message,messageCheck))
        { 
            message.channel.startTyping();
            Trigger(message);
            message.channel.stopTyping();
        }
    }
    catch(e){
        console.log('~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~SOMETHING WENT WRONG!!!!!!!!!!!!!!!!!!!')
        console.log(message.content);
        console.log('~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~SOMETHING WENT WRONG!!!!!!!!!!!!!!!!!!!')
    }
})

function Trigger(message){
    //var input = 'UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!\n\n';
    var input = `Hello there! I'm UwU-bot! I saw your cool message over there and it didn't seem right. Let me help you out and fix that for you!\n`;
    input = (UwU(message, input));
    //input = input.concat('\n\nUwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!UwU!');
    logInput(message, input);
    message.channel.send(input);
}

function UwU(message, input)
{
    input = input.concat("Didn't you mean: \n" + message.content);
    input = input.replace(/r/g,'w');
    input = input.replace(/R/g,'W');
    input = input.replace(/l/g,'w');
    input = input.replace(/L/g,'W');
    input = input.replace(/ove/g,'uv');
    input = input.replace(/OVE/g,'UV');
    return input;
}
function willTrigger(message,messageCheck)
{
    var random_boolean = Math.random() <= triggerChance;
    if (!messageCheck.includes(`r`) || !messageCheck.includes(`l`)) return false;
    if (message.content.includes(PREFIX) && random_boolean == true && (messageCheck.length > lengthThreshhold || ((hasEnoughR(messageCheck) > rThreshhold || hasEnoughL(messageCheck) > lThreshhold))))
    {
        return true;
    }
    else
    {
        return false;
    }
}

function altTriggers(message,checkThis){
    if (checkThis === (`<@!${bot.user.id}>`)) //for some reason, copying a tag from discord back into discord results in a different ID. This code only works if the bot was manually tagged and the tag wasnt copied
    {
        message.channel.startTyping();
        wuvUser(message);
        message.channel.stopTyping();
        return true;
    }
    else if(checkThis === `<@!${bot.user.id}> sing`)
    {
        var outputMsg = `Suwe thing!! \nhttps://www.youtube.com/watch?v=h6DNdop6pD8\nENJOY! UwU`
        logInput(message, outputMsg);
        message.channel.send(outputMsg);
        return true;
    }
    else if(checkThis.includes(`<@!${bot.user.id}>`))
    {
        message.channel.startTyping();
        if(message.content.includes('r',0) || message.content.includes('l',0))
        {
            wuvUser(message,true);
            Trigger(message);
        }
        else
        {
            wuvUser(message);
        }
        message.channel.stopTyping();
        return true;
    }
    return false;
}

function countTheR(text)
{
    return text.match(/r/gi).length;
}

function hasEnoughR(input)
{
    var text = input.split(' ')
    var hasAnR = 0;
    for(var i=0; i < text.length; i++)
    {
        if(text[i].includes('r') || text[i].includes('R'))
        {
            hasAnR++;
        }
    }
    return (hasAnR/text.length);
}
function hasEnoughL(input)
{
    var text = input.split(' ')
    var hasAnR = 0;
    for(var i=0; i < text.length; i++)
    {
        if(text[i].includes('l') || text[i].includes('L'))
        {
            hasAnR++;
        }
    }
    return (hasAnR/text.length);
}

function wuvUser(message, more = false)
{
    var userID = message.author.id;

    var returnMsg = `AAAWWWWWW!!!1! I WUV U 2 <@!${userID}>!!! * x3 NUZZLES YOUW NECKY WEKY* UwU!`
    if(more === true){
        returnMsg = returnMsg.concat(`\nI'ww do the west of da msg now! xoxo`)
    }
    logInput(message, returnMsg);
    message.channel.send(returnMsg);
}

function logInput(message, output)
{
    var userName = message.author.tag;
    var serverName = message.guild.name;
    var inputMessage = message.content;
    var recievedChannel = message.channel.name;

    console.log();
    console.log(`New message from: @${userName}`);
    console.log(`Message sent from Server: ${serverName}`);
    console.log(`Sent in channel: '${recievedChannel}'`);
    console.log();
    console.log(`Input: ${inputMessage}`);
    console.log("______________________________________________________")
    console.log(`Output: \n${output}`)
    console.log();
    console.log(`#################################################################################################`)
}

function checkWhetherToRespond(message)
{
    if (message.content.includes(bot.id)) return true;
    if (!message.content.includes(PREFIX)) return false;
    if (message.author.bot) return false; //ignores itself and other bots
    if (message.content.length<=1) return false;//check for length of message
    if (message.content.includes('https://')) return false;
    if (message.content.includes('www.')) return false;
    return true;
}