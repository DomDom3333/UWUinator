const Collector = require('../../BotFunctions/MessageCollector.js');
const LOG = require('../../BotFunctions/Log.js');
const CONFIG = require('../../config.json');
const COLLECTOR = require('../../BotFunctions/MessageCollector.js');
const Utils = require('../../BotFunctions/Utils.js');
const CommandConfigFileName = 'uwu.json'
const COMMANDCONFIG = require(`./${CommandConfigFileName}`);
var selfTag = 

module.exports = {
    name: 'PassiveCommand',
    description: "This is the UWU Command",
    type:'Passive',
    enabled: true, //if false, command will be disabled
    execute(message,args){
        if(this.enabled){
            var ServerIndex = GetServerIndex(message.guild.id)
            var messageCheck = message.content.toLowerCase();
            if(altTriggers(message,messageCheck,args)) return;
            if (willTrigger(message,messageCheck,ServerIndex))
            { 
                Trigger(message);
            }            
        }
        else{
            Collector.Add("This command is currently DISABLED");
        }
    }
}

//#region Command
function willTrigger(message,messageCheck,Index)
{
    var isBoundUser = COMMANDCONFIG.Servers[Index].BoundUsers.includes(message.author.id);
    if (isBoundUser) return true;
    if ((message.content.split(' ').length -1) < 2) return false;
    if (!messageCheck.includes('r') && !messageCheck.includes('l')) return false;
    var randNumber = Math.random()
    var random_boolean = randNumber <= COMMANDCONFIG.Servers[Index].TriggerChance;

    var prefixAndChance = (message.content.includes(CONFIG.Prefix) && random_boolean == true);
    var lengthAndAmount = (messageCheck.length > CONFIG.lengthThreshhold || ((hasEnoughR(messageCheck) > CONFIG.rThreshhold || hasEnoughL(messageCheck) > CONFIG.lThreshhold)));

    if (prefixAndChance && lengthAndAmount && !CONFIG.Admins.includes(message.author.id))
    {
        return true;
    }
    else
    {
        return false;
    }
}
function altTriggers(message,checkThis,args){
    var tagText = Utils.getValidBotTag(message,checkThis);
    if (tagText != false && checkThis === tagText) //for some reason, copying a tag from discord back into discord results in a different ID. This code only works if the bot was manually tagged and the tag wasnt copied
    {
        wuvUser(message);
        return true;
    }
    else if(tagText != false && checkThis === `${tagText} sing` || checkThis === `${tagText} sing`) 
    {
        var outputMsg = `Suwe thing!! \nhttps://www.youtube.com/watch?v=h6DNdop6pD8\nENJOY! UwU`
        logInput(message, outputMsg);
        COLLECTOR.Add(outputMsg);
        return true;
    }
    else if(tagText != false && checkThis === `${tagText} info` || checkThis === `${tagText} help`)
    {
        var outputMsg = `YAY!! Pwease Vote fow me!! \nhttps://top.gg/bot/776864557775585296\nHere is my Discord server too: https://discord.gg/9g9xMkKY2N\nENJOY! UwU`
        logInput(message, outputMsg);
        COLLECTOR.Add(outputMsg);
        return true;
    }
    else if (tagText != false && checkThis.startsWith(`${tagText}`) && args.length > 1 && (args[1] == "chance" || args[1] == "bind" || args[1] == "unbind")){
        switch (args[1]) {
            case 'chance':
                if (args.length > 2 && message.member.hasPermission("ADMINISTRATOR") || CONFIG.Admins.includes(message.author.id)) {
                    var newChance = Number(args[2]);
                    ChangeTriggerChance(newChance,message.guild.id);
                    COLLECTOR.Add("Twiggew chance updated");
                    return true;
                } 
                else {
                    if (args.length < 2) {
                        COLLECTOR.Add("No new Chance specified.");
                    }
                    else if (!message.member.hasPermission("ADMINISTRATOR")) {
                        COLLECTOR.Add("Onwy Administwators can change the chance of UwUBot wepwying!");                    
                    }
                    return true;
                }
                break;
            case 'bind':
                if (message.mentions.members.size > 1  &&  message.member.hasPermission("ADMINISTRATOR") || CONFIG.Admins.includes(message.author.id)) {
                    var newBind = message.mentions.users.array()[1].id;
                    if (newBind) {
                        AddBoundUser(newBind,message.guild.id);
                        COLLECTOR.Add("Usew added to Bound usews");
                        return true;                        
                    }
                    else{
                        Collector.Add("You canonly bind specific users.");
                    }
                }
                else{
                    if (message.mentions.members.size < 1) {
                        COLLECTOR.Add("No usew mentioned!");                    
                    }
                    else if (!message.member.hasPermission("ADMINISTRATOR")) {
                        COLLECTOR.Add("Onwy Administwators can Bind Usews!");                    
                    }
                    return true;
                }
                break;
            case 'unbind':
                if (message.mentions.members.size > 1  && message.member.hasPermission("ADMINISTRATOR") || CONFIG.Admins.includes(message.author.id)) {
                    var newUnbind = message.mentions.users.array()[1].id;
                    if (newUnbind) {
                        RemoveBoundUser(newUnbind,message.guild.id);
                        COLLECTOR.Add("Usew wemoved fwom bound usews");
                        return true;                        
                    }
                    else{
                        Collector.Add("You canonly unbind specific users.");
                    }
                }
                else{
                    if (message.mentions.members.size < 1) {
                        COLLECTOR.Add("No usew mentioned!");                    
                    }
                    else if (!message.member.hasPermission("ADMINISTRATOR")) {
                        COLLECTOR.Add("Onwy Administwatows can Unbind Usews!");                    
                    }
                    return true;
                }
                break;
            default:
                break;
        }
    }
    else if(checkThis.includes(`${tagText}`))
    {
        //wuvUser(message);
        Trigger(message);      
        return true;
    }
    return false;
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
//#endregion

//#region Triggers
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
//#endregion

//#region Config
function ChangeTriggerChance(newChance,serverID){
    var index = GetServerIndex(serverID);
    COMMANDCONFIG.Servers[index].TriggerChance = newChance;
    SaveCommandConfig();
}
function AddBoundUser(userID,serverID){
    var index = GetServerIndex(serverID);
    var isBound = isUserBound(userID,index);
    if (isBound == false) {
        COMMANDCONFIG.Servers[index].BoundUsers[COMMANDCONFIG.Servers[index].BoundUsers.length] = userID;
        SaveCommandConfig();
    }
    else{
        COLLECTOR.Add("User is already bound.");
    }
}
function RemoveBoundUser(userID,serverID){
    var index = GetServerIndex(serverID);
    var isBound = isUserBound(userID,index);
    if (isBound === false) {
        COLLECTOR.Add("User is not bound.");
    }
    else{
        COMMANDCONFIG.Servers[index].BoundUsers.splice(isBound,1);
        SaveCommandConfig();
    }
}
function isUserBound(userID, serverIndex){
    for (let i = 0; i < COMMANDCONFIG.Servers[serverIndex].BoundUsers.length; index++) {
        const element = COMMANDCONFIG.Servers[serverIndex].BoundUsers[i];
        
        if (element == userID) {
            return i;
        }
        else{
            return false;
        }
    }
    return false;
}
function GetServerIndex(serverID){
    for (let i = 0; i < COMMANDCONFIG.Servers.length; i++) {
        const element = COMMANDCONFIG.Servers[i];
        if (element.ServerID === serverID) {
            return i;
        }
    }
    CreateServer(serverID);
    return COMMANDCONFIG.Servers.length-1;
}
function CreateServer(serverID){
    COMMANDCONFIG.Servers[COMMANDCONFIG.Servers.length] = {ServerID:`${serverID}`,TriggerChance:0.35,BoundUsers:[]}
    SaveCommandConfig();
}
function SaveCommandConfig(){
    Utils.saveJsonFile(COMMANDCONFIG, `./Commands/Passive/${CommandConfigFileName}` ,true)
}
//#endregion
