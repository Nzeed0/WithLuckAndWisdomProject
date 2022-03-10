﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using NOBOIShooter.Controls;
using NOBOIShooter.GameObjects;
using System;
using System.Collections.Generic;
using NOBOIShooter.Data;

namespace NOBOIShooter.Screens
{
    //Game screen
    public class GameScreen : AScreen
    {
        private const int DEAD_LINE_THICK = 5;
        private BallTexture _ballTexture;
        private Texture2D _bubbleImg;
        private Texture2D _shooterImg;
        private Texture2D _backIcon;
        private Texture2D _background;
        private Texture2D _pen;

        private SpriteFont _textFront;

        private SoundEffect _gameSfxBg, _gameSfxEnd, _gameSfxWin;
        private SoundEffectInstance _sfxBgInstance, _sfxEndInstance2, _sfxEndInstance3;


        private BallGridManager _bord;
        private Player _player;

        private Button _backButton;

        private ScoreData _scoreTable;
        private bool _gameScoreSave = false;

        private Rectangle _fullScreen;
        private Rectangle _gameScreen;
        private Rectangle _deadline;
        private Color _gameColorBackground;
        private Vector2 _textPosition;

        public GameScreen(Main game, GraphicsDevice graphicsDevice, ContentManager content)
            : base(game, graphicsDevice, content)
        {
            //State = GameState.init;
            _textFront = _content.Load<SpriteFont>("Fonts/Font");
            _backIcon = _content.Load<Texture2D>("Controls/BackButton");
            _background = content.Load<Texture2D>("Backgrouds/gameBackground");
      
            _bubbleImg = _content.Load<Texture2D>("Item/bubble");

            _gameSfxBg = content.Load<SoundEffect>("BGM/GameScreenBGM");
            _gameSfxEnd = content.Load<SoundEffect>("BGM/GameOverBGM");
            _gameSfxWin = content.Load<SoundEffect>("BGM/VictoryBGM");

            _sfxBgInstance = _gameSfxBg.CreateInstance();
            _sfxBgInstance.Volume = Singleton.Instance.BGMVolume;
            _sfxEndInstance2 = _gameSfxEnd.CreateInstance();
            _sfxEndInstance2.Volume = Singleton.Instance.SFXVolume;
            //_sfxBgInstance.IsLooped = true;
            _sfxEndInstance3 = _gameSfxWin.CreateInstance();
            _sfxEndInstance3.Volume = Singleton.Instance.SFXVolume;
            //_sfxBgInstance.IsLooped = true;

            _sfxBgInstance.Play();


            _shooterImg = _content.Load<Texture2D>("Item/hand-gun");
            _pen = new Texture2D(graphicsDevice, 1, 1);
            _pen.SetData(new[] { Color.White });

            _backButton = new Button(_backIcon, content)
            {
                Position = new Vector2(1200, 20),
            };
            _backButton.Click += BackButton_Click;

            _ballTexture = new BallTexture();
            _ballTexture.Add(_bubbleImg);
            _ballTexture.Add(_bubbleImg,Color.Red);
            _ballTexture.Add(_bubbleImg,Color.Green);
            _ballTexture.Add(_bubbleImg,Color.Blue);
            _bord = new BallGridManager(_ballTexture);
            _player = new Player(_bord, _shooterImg, _ballTexture, _pen);
            _scoreTable = new ScoreData();
            _scoreTable.LoadSave();

            _fullScreen = new Rectangle(0, 0, Singleton.Instance.ScreenWidth, Singleton.Instance.ScreenHeight);
            _gameScreen = new Rectangle((int)_bord.Position.X, 0, _bord.Width, Singleton.Instance.ScreenHeight);
            int line = (int)(_bord.Position.Y + _bord.Height - 2 * _bord.RowHeight + _bord.TileHeight );
            _deadline = new Rectangle((int)_bord.Position.X, line, _bord.Width, DEAD_LINE_THICK);

            _gameColorBackground = new Color(Color.Black, 0.5f);
            _textPosition = new Vector2(1000, 180);

        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            // Draw Backgrounds
            spriteBatch.Draw(_background, _fullScreen, Color.White);
            // Draw Backgrounds
            spriteBatch.Draw(_pen, _gameScreen, _gameColorBackground);
            spriteBatch.Draw(_pen, _deadline, Color.DarkRed);

            _backButton.Draw(gameTime, spriteBatch);
            _player.Draw(spriteBatch, gameTime);
            _bord.Draw(spriteBatch, gameTime);

            if (!_bord.GamePause)
            {
                spriteBatch.DrawString(_textFront, "Score \n\n " + _bord.GameScore, _textPosition, Color.Black);
                //Debug.WriteLine("Score : " + _bord.GameScore);
            }

            // Draw fade out
            if (_bord.GameWin || _bord.GameEnd)
            {
                if(!_gameScoreSave)
                {
                    _scoreTable.Add(new Score(_bord.GameScore, DateTime.Now));
                    _scoreTable.Sort();
                    _scoreTable.SaveGame();
                    _gameScoreSave = true;
                    //Debug.WriteLine("Save Score ?: " + _bord.GameScore);

                }

                spriteBatch.Draw(_pen, _fullScreen, new Color(Color.Black, .6f));

                string gameScore = "Your score is " + _bord.GameScore;
                spriteBatch.DrawString(_textFront, gameScore,
                    new Vector2((float)(Singleton.Instance.ScreenWidth - _textFront.MeasureString(gameScore).X) / 2, 260f), Color.White);

                _sfxBgInstance.Pause();
            }


            if (_bord.GameEnd)
            {
                _sfxEndInstance2.Play();
                string gameEnding = "Game Ending \n\n Thank for playing.";
                spriteBatch.DrawString(_textFront, gameEnding,
                    new Vector2((float)(Singleton.Instance.ScreenWidth - _textFront.MeasureString(gameEnding).X)/2, 100f), Color.White);
            }

            if (_bord.GameWin)
            {
                _sfxEndInstance3.Play();
                string gameEnding = "You Won \n\n Want to play again?";
                spriteBatch.DrawString(_textFront, gameEnding,
                    new Vector2((float)(Singleton.Instance.ScreenWidth - _textFront.MeasureString(gameEnding).X) / 2, 100f), Color.White);
            }

            spriteBatch.End();
        }

        public override void PostUpdate(GameTime gameTime)
        {
            //
        }

        public override void Update(GameTime gameTime)
        {
            _backButton.Update(gameTime);
            _bord.Update(gameTime);
            _player.Update(gameTime);
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            //Singleton.Instance.removeBubble.Clear();
            //Singleton.Instance.Shooting = false;
            _bord.ClearGame();
            _sfxBgInstance.Dispose();
            _sfxEndInstance2.Dispose();
            _sfxEndInstance3.Dispose();
            _game.ChangeScreen(ScreenSelect.Menu);
        }
    }
}
