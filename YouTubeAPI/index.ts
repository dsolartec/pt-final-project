import express from 'express';
import cors from 'cors';
import ytdl from 'ytdl-core';

const app = express();

app.use(cors());

app.get('/info/*', async (req, res) => {
  const { v: id } = req.query;

  if (typeof id === 'string') {
    if (!ytdl.validateID(id)) {
      return res.status(400).send('URL inválida');
    }

    try {
      const info = await ytdl.getBasicInfo(`https://youtube.com/watch?v=${id}`);

      res.contentType('application/json').jsonp({
        artists: info.videoDetails.author.name,
        id,
        name: info.videoDetails.title,
      });
    } catch (error) {
      console.error(error);

      res.status(500).send('Ha ocurrido un error');
    }

    return;
  }

  res.status(500).send('Petición no válida');
});

app.get('/stream/*', (req, res) => {
  const { v: id } = req.query;

  if (typeof id === 'string') {
    if (!ytdl.validateID(id)) {
      return res.status(400).send('URL inválida');
    }

    ytdl(`https://youtube.com/watch?v=${id}`, { filter: 'audioonly' }).pipe(res);
    return;
  }

  res.status(500).send('Petición no válida');
});

app.listen(3005, () => {
  console.log('App listening on port :3005');
});
