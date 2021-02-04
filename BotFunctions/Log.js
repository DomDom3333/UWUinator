const CONFIG = require('../config.json');
const COLLECTOR = require('./MessageCollector.js');
const fs = require('fs');
const readLastLines = require('read-last-lines');
const UTILS = require('./Utils');

var messagesRecieved = 0;
var messagesSent = 0;
var serversJoined = 0;
var servingUsers = 0;

var today = new Date();
var now = new Date();
UpdateTimes();


module.exports = {
    error(errorMessage,error = ''){
        UpdateTimes();
        var logText = `\n\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\n${now} - ${errorMessage}\n${error}\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\n\n`;

        writeToConsole(logText);
        writeToFile(logText);
    },

    info(infoMessage){
        UpdateTimes();
        var logText = `${now} - ${infoMessage}\n--------------------------------------------------------------------------------------------------------------`;

        writeToConsole(logText);
        writeToFile(logText);
    },
    async review(numOfLines = 20){
        UpdateTimes();
        return readLog(numOfLines) ;
    }
}

function UpdateTimes(){
    today = new Date();
    var dd = String(today.getDate()).padStart(2, '0');
    var mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
    var time = String(today.getHours() + ':' + today.getMinutes() + ':' + today.getSeconds());
    var yyyy = today.getFullYear();
    
    today = dd + '_' + mm + '_' + yyyy;
    now = time + ' | ' + dd + '/' + mm + '/' + yyyy;
}

function writeToFile(TextToWrite){
    fs.appendFile(`${CONFIG.LogPath}/${today}.txt`, TextToWrite,function (err){
        if (err) throw err;
    });
    if (fs.existsSync(`${CONFIG.LogPath}/${today}`)){
    }
}
function writeToConsole(TextToWrite){
    console.log(TextToWrite);
}

function readLog(numOfLines = 35){
    var LogPromise = '';
    try{
        numOfLines = parseInt(numOfLines,10)
        if(!Number.isInteger(numOfLines)){
            COLLECTOR.Add('Please make sure you input a number of lines to review. (eg: ReviewLog 50)');
            return LogPromise;
        }
    }
    catch(err){
        COLLECTOR.Add('Please make sure you input a number of lines to review. (eg: ReviewLog 50)');
        return LogPromise;
    }
    LogPromise = readLastLines.read(`${CONFIG.LogPath}/${today}.txt`, numOfLines);

    return LogPromise;
}
