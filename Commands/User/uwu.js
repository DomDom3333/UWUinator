const Collector = require('../../BotFunctions/MessageCollector.js');
const LOG = require('../../BotFunctions/Log.js');
const CONFIG = require('../../config.json');
const COLLECTOR = require('../../BotFunctions/MessageCollector.js');



module.exports = {
    name: 'PassiveCommand',
    description: "This is the UWU Command",
    enabled: true, //if false, command will be disabled
    execute(message,args){
        if(this.enabled){
            var messageCheck = message.content.toLowerCase();
            if(altTriggers(message,messageCheck)) return;
            if (willTrigger(message,messageCheck))
            { 
                Trigger(message);
            }            
        }
        else{
            Collector.Add("This command is currently DISABLED");
        }
    }
}


function Trigger(message){
    var input = `Hello there! I'm UwU-bot! I saw your cool message over there and it didn't seem right. Let me help you out and fix that for you!\n`;
    input = (UwU(message, input));
    logInput(message, input);
    COLLECTOR.Add(input);
}

function UwU(message, input)
{
    input = input.concat("Didn't you mean: \n>>> " + message.content);
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
    if ((message.content.split(' ').length -1) < 2) return false;
    if (!messageCheck.includes('r') || !messageCheck.includes('r')) return false;
    var random_boolean = Math.random() <= CONFIG.triggerChance;
    if (message.content.includes(CONFIG.Prefix) && random_boolean == true && (messageCheck.length > CONFIG.lengthThreshhold || ((hasEnoughR(messageCheck) > CONFIG.rThreshhold || hasEnoughL(messageCheck) > CONFIG.lThreshhold))))
    {
        return true;
    }
    else
    {
        return false;
    }
}

function altTriggers(message,checkThis){
    if (checkThis === (`<@!${message.guild.me.id}>`)) //for some reason, copying a tag from discord back into discord results in a different ID. This code only works if the bot was manually tagged and the tag wasnt copied
    {
        wuvUser(message);
        return true;
    }
    else if(checkThis === `<@!${message.guild.me.id}> sing`)
    {
        var outputMsg = `Suwe thing!! \nhttps://www.youtube.com/watch?v=h6DNdop6pD8\nENJOY! UwU`
        logInput(message, outputMsg);
        COLLECTOR.Add(outputMsg);
        return true;
    }
    else if(checkThis === `<@!${message.guild.me.id}> info`)
    {
        var outputMsg = `YAY!! Pwease Vote fow me!! \nhttps://top.gg/bot/776864557775585296\nENJOY! UwU`
        logInput(message, outputMsg);
        COLLECTOR.Add(outputMsg);
        return true;
    }
    else if(checkThis.includes(`<@!${message.guild.me.id}>`))
    {
        //wuvUser(message);
        Trigger(message);      
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
    COLLECTOR.Add(returnMsg);
}

function logInput(message, output, tag)
{
    if (tag) {
        LOG.info(`I got Tagged O_O!\n- Server: ${message.guild.name}\n- User: ${message.author.username}\n- Content: ${message.content}`);
    }
    else{
        LOG.info(`I UwUd something!\n- Server: ${message.guild.name}\n- User: ${message.author.username}\n- Content: ${message.content}`);
    }
}
