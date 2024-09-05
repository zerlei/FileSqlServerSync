// import WebSocket from 'ws';

// const wsUrl = 'wss://toolin.cn/echo';
// const ws = new WebSocket(wsUrl);

// ws.on('open', () => {
//     console.log('Connected to WebSocket server');
//     ws.send("赵磊f",(err)=>{
//         console.log(err)
//     })
    
// });

// ws.on('message', (message) => {
//     var str = message.toString('utf8')
//     if(str.includes("赵磊f")) {
//         ws.close()
//     }
//     console.log('Received message:',str);
// });

// ws.on('close', () => {
//     console.log('Disconnected from WebSocket server');
// });

import chalk from 'chalk';
function logProgress(current, total) {
    const progressPercentage = (current / total) * 100;
    var str = `Progress: ${progressPercentage.toFixed(2)}%\r`
    var x = chalk['red'](str);
    process.stdout.write(x);
}

// Example usage:
setInterval(() => {
    logProgress(Math.floor(Math.random() * 100), 100);
}, 100); // Update progress every 100 milliseconds with a random value
