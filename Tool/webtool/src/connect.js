class ConnectPipe {
  #websocket;
  //在这里打断点可能会导致debug错误，然后浏览器打不开页面， 这是为啥？
  constructor() {
    //Id,Msgtype,callback
    // this.#websocket = new WebSocket(`ws://${window.location.host}`)
  }
  OpenPipe(config, MsgCb) {
    this.config = config;

    // var webSocUrl = `ws://${window.location.host}:${window.location.port}/websoc?Name=${config.Name}`
    var webSocUrl = "ws://127.0.0.1:6818/websoc?Name=Test"
    this.#websocket = new WebSocket(webSocUrl);
    this.#websocket.onopen = (event) => {
    //   console.warn("websocket connected!");
      this.#websocket.send(JSON.stringify(this.config));
    };
    this.#websocket.onmessage = (event) => {
      console.log(event.data)

    };
    this.#websocket.onclose = (event) => {
      console.warn(event.reason)
    };
    this.#websocket.onerror = (e) => {
      console.error(e)
      if (this.#websocket.readyState) {
        //bla bla
      }
    };
  }
}


export default ConnectPipe;
