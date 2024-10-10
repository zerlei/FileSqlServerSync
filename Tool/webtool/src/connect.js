class ConnectPipe {
  #websocket;
  //在这里打断点可能会导致debug错误，然后浏览器打不开页面， 这是为啥？
  constructor() {
    //Id,Msgtype,callback
    // this.#websocket = new WebSocket(`ws://${window.location.host}`)
  }
  #OpenPipe(config, MsgCb) {
    this.config = config;

    var webSocUrl = `ws://${window.location.host}:${window.location.port}/websoc?Name=${config.Name}`
    this.#websocket = new WebSocket(webSocUrl);
    this.#websocket.onopen = (event) => {
    //   console.warn("websocket connected!");
      this.#websocket.send(JSON.stringify(this.config));
    };
    this.#websocket.onmessage = (event) => {

        

    };
    this.#websocket.onclose = (event) => {};
    this.#websocket.onerror = (e) => {
      if (this.#websocket.readyState) {
        //bla bla
      }
    };
  }
}

let cPipe = new ConnectPipe();

export default cPipe;
