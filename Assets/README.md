# Super Pickers

## How to run?
### Prerequisites
The easiest way to run the application is by first installing `http-server` using npm 
(https://www.npmjs.com/package/http-server):
```bash
npm install -g http-server
```

### Running the application
Navigate to the directory where your build is located and run:
```bash
http-server -p 8544 --cors
```

After this, navigate to http://localhost:8544 and the application will start.

## Coniguration
In order for the visualization to start, you need to put a file called `sample_data.json`
in the build directory.