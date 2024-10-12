class ConnectPipe {
  #websocket;
  //在这里打断点可能会导致debug错误，然后浏览器打不开页面， 这是为啥？
  constructor() {
    //Id,Msgtype,callback
    // this.#websocket = new WebSocket(`ws://${window.location.host}`)
  }
  OpenPipe(config, MsgCb) {
    var webSocUrl = `ws://${window.location.host}/websoc?Name=${config.Name}`
    // var webSocUrl = "ws://127.0.0.1:6818/websoc?Name=Test";
    this.#websocket = new WebSocket(webSocUrl);
    this.#websocket.onopen = (event) => {
      var starter = {
        Body: JSON.stringify(config),
        Type: 1,
        Step: 1,
      };
      //   console.warn("websocket connected!");
      this.#websocket.send(JSON.stringify(starter));
    };
    this.#websocket.onmessage = (event) => {
      // console.log(event.data);
      MsgCb(JSON.parse(event.data))
    };
    this.#websocket.onclose = (event) => {

      console.warn(event)
      MsgCb({
        Type: 0,
        Step: 8,
        Body:event.reason
      })
      
    };
    this.#websocket.onerror = (e) => {
      console.error(e);
      MsgCb({
        Type: 0,
        Body: "异常错误，查看 Console",
        Step: 7,
      });
    };
  }
  ClosePipe()  {
    this.#websocket.close();
  }

}

export default ConnectPipe;
