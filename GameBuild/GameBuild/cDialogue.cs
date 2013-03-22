﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameBuild
{
    public class cDialogue
    {
        Texture2D portrait;
        Texture2D textBox;
        SpriteFont font;

        public Rectangle portraitPos;
        public Rectangle textBoxPos;

        int width = 0, height = 0;

        public bool isTalking;

        int selection = 1;
        int currentStatement = 0;

        KeyboardState oldState;
        KeyboardState currentState;

        string[] currentLines;

        DialogueManager dialogueManager;

        public cDialogue(Texture2D portrait, Texture2D textBox, Game1 game, SpriteFont font, string dialogueFileName)
        {
            this.portrait = portrait;
            this.textBox = textBox;
            this.font = font;

            int screenWidth = game.graphics.PreferredBackBufferWidth;
            int screenHeight = game.graphics.PreferredBackBufferHeight;

            portraitPos = new Rectangle(game.graphics.PreferredBackBufferWidth - width, game.graphics.PreferredBackBufferHeight - height, width, height);
            textBoxPos = new Rectangle(screenWidth/2 - textBox.Width/2, screenHeight - screenHeight/4, textBox.Width, textBox.Height);

            dialogueManager = new DialogueManager(@"Content\npc\dialogue\" + dialogueFileName + ".txt");
            GetLines(0);
        }

        public void Update()
        {
            oldState = currentState;
            currentState = Keyboard.GetState();
            
            if (currentState.IsKeyDown(Keys.Up) && oldState.IsKeyUp(Keys.Up))
            {
                selection -= 1;
                if (selection < 1)
                {
                    selection = currentLines.Length - 1;
                }
            }
            else if (currentState.IsKeyDown(Keys.Down) && oldState.IsKeyUp(Keys.Down))
            {
                selection += 1;
                if (selection > currentLines.Length-1)
                {
                    selection = 1;
                }
            }
            if (currentState.IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter))
            {
                GotoNextStatement();
                selection = 1;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (isTalking && portrait != null)
            {
                spriteBatch.Draw(portrait, portraitPos, Color.White);
                spriteBatch.Draw(textBox, textBoxPos, Color.White);
                for (int i = 1; i < currentLines.Length; i++)
                {
                    spriteBatch.DrawString(font, currentLines[i], new Vector2(textBoxPos.X + 100, textBoxPos.Y + 40 + i*20), Color.DarkOrange);
                }
                if (currentLines.Length>1)
                {
                    spriteBatch.DrawString(font, currentLines[selection], new Vector2(textBoxPos.X + 100, textBoxPos.Y + 40 + selection*20), Color.Red);
                }
                spriteBatch.DrawString(font, currentLines[0], new Vector2(textBoxPos.X + 100, textBoxPos.Y + 10), Color.Black);
            }
        }

        private void GotoNextStatement()
        {
            int index = dialogueManager.GetNextStatementIndex(currentStatement, selection);
            if (index > -1)
            {
                currentLines = dialogueManager.GetDialogueLinesFromIndex(index);
                currentStatement = index;
            }
            else
            {
                ResetDialogue();
                Console.WriteLine("FREE");
            }
        }

        private void GetLines(int index)
        {
            currentLines = dialogueManager.GetDialogueLinesFromIndex(index);
        }

        private void ResetDialogue()
        {
            isTalking = false;
            selection = 1;
            currentLines = dialogueManager.GetDialogueLinesFromIndex(0);
            currentStatement = 0;
        }
    }
}
