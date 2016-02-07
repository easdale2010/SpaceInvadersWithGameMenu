using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace gamemenu
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Screen height and width
        int displaywidth = 800;
        int displayheight = 480;
        
     
        int ystop = 0;

        int lives = 5;
        int score = 0;
//        int highscore = 0;

        struct graphics2d
        {
            public Texture2D image;
            public Vector3 position;
            public Vector3 oldposition;
            public Rectangle rect;
            public Vector2 origin;
            public float size;
            public Vector3 velocity;
            public BoundingBox bbox;
            public BoundingSphere bsphere;
            public float power;
            public Boolean visible;
            public int sheildlives;
        }

        graphics2d background;
        const int columns = 10;
        const int rows = 4;

        graphics2d[,] invader = new graphics2d[columns, rows];
        graphics2d gameoverimage;
        graphics2d[] sheild = new graphics2d[4];

        int invadercounter;
        graphics2d laserbase;
        graphics2d goodlaser;
        graphics2d[] badlaser = new graphics2d[3];
        float lasercounter = 0;
        SoundEffect goodlasersound, badlasersound, goodhit, badhit, gameoversound;
        SpriteFont mainfont;


        // Structure for static 2D graphics
        struct graphic2d
        {
            public Texture2D image;                 // Texture to hold image
            public Rectangle rect;                  // Rectangle to hold position & size of the image
        }

        // Structure for moving 2D graphics
        struct sprite2d
        {
            public Texture2D image;         		// Texture which holds image
            public Vector3 position; 		 	    // Position on screen
            public Vector3 oldposition;             // Old position before collisions
            public Rectangle rect;          		// Rectangle to hold size and position
            public Vector2 origin;          		// Centre point
            public BoundingSphere bsphere;  		// Bounding sphere
            public BoundingBox bbox;                // Bounding Box
            public float size;                      // Size ratio of object
            public Vector3 velocity;        		// Velocity (Direction and speed)
            public float rotation;          	    // Amount of rotation to apply
            public Boolean visible;    		        // Should object be drawn true or false
        }

        Boolean gameover = false;   // Is the game over TRUE or FALSE?      
        float gameruntime = 0;      // Time since game started

        Random randomiser = new Random();       // Variable to generate random numbers

        int gamestate = -1;         // Current game state

        GamePadState[] pad = new GamePadState[4];       // Array to hold gamepad states
        KeyboardState keys;                             // Variable to hold keyboard state
        MouseState mouse;                               // Variable to hold mouse state
        Boolean released = true;                        // Check for sticks or buttons being released

        sprite2d mousepointer1, mousepointer2;          // Sprites to hold a mouse pointer
        const int numberofoptions=4;                    // Number of main menu options
        sprite2d[,] menuoptions = new sprite2d[numberofoptions,2]; // Array of sprites to hold the menu options
        int optionselected = -1;                         // Current menu option selected

        const int numberofhighscores = 10;                              // Number of high scores to store
        int[] highscores = new int[numberofhighscores];                 // Array of high scores


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Set the screen resolution
//            this.graphics.PreferredBackBufferWidth = 800;
//            this.graphics.PreferredBackBufferHeight = 600;
            this.graphics.PreferredBackBufferWidth = displaywidth;
            this.graphics.PreferredBackBufferHeight = displayheight;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            displaywidth = graphics.GraphicsDevice.Viewport.Width;
            displayheight = graphics.GraphicsDevice.Viewport.Height;
            //graphics.ToggleFullScreen(); // Put game into full screen mode

            base.Initialize();
        }

        // Method to load sprites and set them up
        void loadsprite(ref sprite2d sprite, string graphicname, int x, int y, float msize)
        {
            sprite.image = Content.Load<Texture2D>(graphicname);    // Load image into texture
            sprite.position = new Vector3((float)x, (float)y, 0);   // Set position
            sprite.rect.X = x;                                      // Set position of draw rectangle x
            sprite.rect.Y = y;                                      // Set position of draw rectangle y
            sprite.origin.X = sprite.image.Width / 2;               // Set X origin to half of width
            sprite.origin.Y = sprite.image.Height / 2;              // Set Y origin to half of height
            sprite.rect.Width = (int)(sprite.image.Width * msize);  // Set the new width based on the size ratio 
            sprite.rect.Height = (int)(sprite.image.Height * msize);// Set the new height based on the size ratio
            sprite.size = msize;                                    // Store size ratio
            // Create Boundingsphere and BoundingBox around the object
            sprite.bsphere = new BoundingSphere(sprite.position, sprite.rect.Width / 2);
            sprite.bbox = new BoundingBox(new Vector3(sprite.position.X - (sprite.rect.Width / 2), sprite.position.Y - (sprite.rect.Height / 2), sprite.position.Z),
                                    new Vector3(sprite.position.X + (sprite.rect.Width / 2), sprite.position.Y + (sprite.rect.Height / 2), sprite.position.Z));
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            mainfont = Content.Load<SpriteFont>("quartz4"); // Load font

            // Load background image and set it to fill the screen
            background.image = Content.Load<Texture2D>("Background for Menus");
            background.rect.X = 0;
            background.rect.Y = 0;
            background.rect.Width = displaywidth;
            background.rect.Height = displayheight;

            // Load both mousepointers
            loadsprite(ref mousepointer1, "X-Games-Cursor", 0, 0, 0.15f);
            loadsprite(ref mousepointer2, "X-Games-Cursor-Highlight", 0, 0, 0.15f);

            // Load menu options graphics
            loadsprite(ref menuoptions[0, 0], "Start-Normal", displaywidth / 2, 100, 1);
            loadsprite(ref menuoptions[0, 1], "Start-Selected", displaywidth / 2, 100, 1);
            loadsprite(ref menuoptions[1, 0], "Options-Normal", displaywidth / 2, 200, 1);
            loadsprite(ref menuoptions[1, 1], "Options-Selected", displaywidth / 2, 200, 1);
            loadsprite(ref menuoptions[2, 0], "High-Score-Normal", displaywidth / 2, 300, 1);
            loadsprite(ref menuoptions[2, 1], "High-Score-Selected", displaywidth / 2, 300, 1);
            loadsprite(ref menuoptions[3, 0], "Exit-Normal", displaywidth / 2, 400, 1);
            loadsprite(ref menuoptions[3, 1], "Exit-Selected", displaywidth / 2, 400, 1);

            // Load in high scores
            if (File.Exists(@"highscore.txt")) // This checks to see if the file exists
            {
                StreamReader sr = new StreamReader(@"highscore.txt");	// Open the file

                String line;		// Create a string variable to read each line into
                for (int i = 0; i < numberofhighscores && !sr.EndOfStream; i++)
                {
                    line = sr.ReadLine();	// Read the first line in the text file
                    line = line.Trim(); 	// This trims spaces from either side of the text
                    highscores[i] = Convert.ToInt32(line);	// This converts line to numeric
                } 
                sr.Close();			// Close the file
            }


            mainfont = Content.Load<SpriteFont>("font");

            background.image = Content.Load<Texture2D>("universe");
            background.rect.X = 0;
            background.rect.Y = 0;
            background.rect.Width = displaywidth;
            background.rect.Height = displayheight;

            gameoverimage.image = Content.Load<Texture2D>("gameover");
            gameoverimage.rect.Y = 0;
            gameoverimage.rect.X = 0;
            gameoverimage.rect.Width = displaywidth;
            gameoverimage.rect.Height = displayheight;

            gameoversound = Content.Load<SoundEffect>("explosion");

            laserbase.image = Content.Load<Texture2D>("goodship");
            laserbase.size = 0.3f;
            laserbase.origin.X = laserbase.image.Width / 2;
            laserbase.origin.Y = laserbase.image.Height / 2;
            laserbase.rect.Width = (int)(laserbase.image.Width * laserbase.size);
            laserbase.rect.Height = (int)(laserbase.image.Height * laserbase.size);
            laserbase.power = 1f;

            for (int i = 0; i < badlaser.Count(); i++)
            {
                badlaser[i].image = Content.Load<Texture2D>("invader_bullet");
                badlaser[i].size = 0.3f;
                badlaser[i].origin.X = badlaser[i].image.Width / 2;
                badlaser[i].origin.Y = badlaser[i].image.Height / 2;
                badlaser[i].rect.Width = (int)(badlaser[i].image.Width * badlaser[i].size);
                badlaser[i].rect.Height = (int)(badlaser[i].image.Height * badlaser[i].size);
                badlaser[i].velocity = new Vector3(0, 10, 0);

            }
            badlasersound = Content.Load<SoundEffect>("laser");
            badhit = Content.Load<SoundEffect>("ballhit");


            goodlaser.image = Content.Load<Texture2D>("tank_bullet_1");
            goodlaser.size = 0.3f;
            goodlaser.origin.X = goodlaser.image.Width / 2;
            goodlaser.origin.Y = goodlaser.image.Height / 2;
            goodlaser.rect.Height = (int)(goodlaser.image.Height * goodlaser.size);
            goodlaser.rect.Width = (int)(goodlaser.image.Width * goodlaser.size);
            goodlaser.velocity = new Vector3(0, -5, 0);

            goodlasersound = Content.Load<SoundEffect>("laser");
            goodhit = Content.Load<SoundEffect>("crash");

            for (int y = 0; y < rows; y++)
                for (int x = 0; x < columns; x++)
                {
                    invader[x, y].image = Content.Load<Texture2D>("invader");
                    invader[x, y].size = 0.12f;
                    invader[x, y].origin.X = invader[x, y].image.Width / 2;
                    invader[x, y].origin.Y = invader[x, y].image.Height / 2;
                    invader[x, y].rect.Width = (int)(invader[x, y].image.Width * 0.14);
                    invader[x, y].rect.Height = (int)(invader[x, y].image.Height * invader[x, y].size);
                    invader[x, y].power = 2;
                }

            sheild[0].image = Content.Load<Texture2D>("sheild1");
            sheild[1].image = Content.Load<Texture2D>("sheild2");
            sheild[2].image = Content.Load<Texture2D>("sheild3");
            sheild[3].image = Content.Load<Texture2D>("sheild4");

            for (int s = 0; s < sheild.Count(); s++)
            {
                sheild[s].origin.X = sheild[s].image.Width / 2;
                sheild[s].origin.Y = sheild[s].image.Height / 2;
                sheild[s].size = 0.4f;
                sheild[s].rect.Width = (int)(sheild[s].image.Width * sheild[s].size);
                sheild[s].rect.Height = (int)(sheild[s].image.Height * sheild[s].size);
            }


            sheild[0].rect.X = 100;
            sheild[0].rect.Y = displayheight - 155;
            sheild[1].rect.X = 300;
            sheild[1].rect.Y = displayheight - 155;
            sheild[2].rect.X = 500;
            sheild[2].rect.Y = displayheight - 155;
            sheild[3].rect.X = 700;
            sheild[3].rect.Y = displayheight - 155;

            for (int s = 0; s < sheild.Count(); s++)
            {
                sheild[s].bsphere = new BoundingSphere(new Vector3((float)sheild[s].rect.X, (float)sheild[s].rect.Y, 0), sheild[s].rect.Width / 2);
            }
            resetgame();

        }



        void resetgame()
        {
            gameover = false;
            lives = 5;

            for (int s = 0; s < sheild.Count(); s++)
            {
                sheild[s].sheildlives = 8;
            }

            laserbase.position = new Vector3(displaywidth / 2, displayheight - 30, 0);
            score = 0;


            for (int y = 0; y < rows; y++)
                for (int x = 0; x < columns; x++)
                {
                    invader[x, y].velocity.X = invader[x, y].power;
                    invader[x, y].velocity.Y = 0;
                }
            resetlevel();
        }

        void resetlevel()
        {
            invadercounter = rows * columns;
            for (int s = 0; s < sheild.Count(); s++)
            {
                sheild[s].visible = true;
                sheild[s].sheildlives += 2;
            }

            int firstx = 40;
            int firsty = 20;

            int xspacing = 10;
            int yspacing = 10;

            for (int y = 0; y < rows; y++)
                for (int x = 0; x < columns; x++)
                {
                    invader[x, y].position.X = firstx + (x * invader[x, y].rect.Width * 1.5f + xspacing);
                    invader[x, y].position.Y = firsty + (y * invader[x, y].rect.Height * 1.2f + yspacing);
                    invader[x, y].visible = true;
                }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            // Save high scores
            StreamWriter sw = new StreamWriter(@"highscore.txt");
            for (int i = 0; i < numberofhighscores; i++)
                sw.WriteLine(highscores[i].ToString());
            sw.Close();

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            pad[0] = GamePad.GetState(PlayerIndex.One);     // Reads gamepad 1
            pad[1] = GamePad.GetState(PlayerIndex.Two);     // Reads gamepad 2
            pad[2] = GamePad.GetState(PlayerIndex.Three);   // Reads gamepad 1
            pad[3] = GamePad.GetState(PlayerIndex.Four);    // Reads gamepad 2
            keys = Keyboard.GetState();                     // Read keyboard
            mouse = Mouse.GetState();                       // Read Mouse
            
            float timebetweenupdates = (float)gameTime.ElapsedGameTime.TotalMilliseconds; // Time between updates
            gameruntime += timebetweenupdates;  // Count how long the game has been running for

            // Read the mouse and set the mouse cursor
            mousepointer1.position.X = mouse.X;
            mousepointer1.position.Y = mouse.Y;
            mousepointer1.rect.X = mouse.X;
            mousepointer1.rect.Y = mouse.Y;
            // Set a small bounding sphere at the center of the mouse cursor
            mousepointer1.bsphere = new BoundingSphere(mousepointer1.position, 2);

            // TODO: Add your update logic here
            switch (gamestate)
            {
                case -1:
                    // Game is on the main menu
                    updatemenu();
                    break;
                case 0:
                    // Game is being played
                    updategame(timebetweenupdates);
                    break;
                case 1:
                    // Options menu
                    updateoptions();
                    break;
                case 2:
                    // High Score table
                    updatehighscore();
                    break;
                default:
                    // Do something if none of the above are selected
                    this.Exit();    // Quit Game
                    break;
            }

            base.Update(gameTime);
        }

        public void updatemenu()
        {
            // Check for mousepointer being over a menu option
            for (int i = 0; i < numberofoptions; i++)
            {
                // Check for up and down on left stick of pad1 for navagating the menu options
                if (released)
                {
                    if (pad[0].ThumbSticks.Left.Y > 0.5f)
                    {
                        optionselected--;
                        released = false;
                    }
                    if (pad[0].ThumbSticks.Left.Y < -0.5f)
                    {
                        optionselected++;
                        released = false;
                    }
                }
                else
                {
                    if (Math.Abs(pad[0].ThumbSticks.Left.Y) < 0.5)
                        released = true;
                }

                // Impose limits on the selectio of menu options 
                if (optionselected < 0) optionselected = 0;
                if (optionselected >= numberofoptions) optionselected = numberofoptions-1;
                
                // Check for mouse over a menu option
                if (mousepointer1.bsphere.Intersects(menuoptions[i, 0].bbox))
                {
                    optionselected = i;
                    if (mouse.LeftButton == ButtonState.Pressed)
                        gamestate = optionselected;
                }

                if (pad[0].Buttons.A == ButtonState.Pressed)
                    gamestate = optionselected;

                if (gamestate == 0)
                    resetgame();
            }
        }

        public void drawmenu()
        {
            spriteBatch.Begin();
            spriteBatch.Draw(background.image, background.rect, Color.White);

            // Draw menu options
            for (int i = 0; i < numberofoptions; i++)
            {
                if (optionselected==i)
                    spriteBatch.Draw(menuoptions[i, 1].image, menuoptions[i, 1].rect, null, Color.White, 0, menuoptions[i, 1].origin, SpriteEffects.None, 0);
                else
                    spriteBatch.Draw(menuoptions[i, 0].image, menuoptions[i, 0].rect, null, Color.White, 0, menuoptions[i, 0].origin, SpriteEffects.None, 0);
            }

            // Draw mouse
            if (optionselected > -1)
            {
                mousepointer2.rect = mousepointer1.rect;
                spriteBatch.Draw(mousepointer2.image, mousepointer2.rect, null, Color.White, 0, mousepointer2.origin, SpriteEffects.None, 0);
            }
            else
                spriteBatch.Draw(mousepointer1.image, mousepointer1.rect, null, Color.White, 0, mousepointer1.origin, SpriteEffects.None, 0);

            spriteBatch.End();
        }

        public void updategame(float gtime)
        {
            // Main game code
         
            const float friction = 0.9f;
     
        

            if (!gameover)
            {
                gameruntime += gtime;

                if (pad[0].Buttons.Back == ButtonState.Pressed)
                    gameover = true;
                
                laserbase.velocity.X += pad[0].ThumbSticks.Left.X * laserbase.power;
                laserbase.position += laserbase.velocity;
                laserbase.velocity *= friction;
                if (laserbase.position.X > displaywidth - laserbase.rect.Width / 2)
                {
                    laserbase.position.X = displaywidth - laserbase.rect.Width / 2;
                    laserbase.velocity.X = 0;
                }
                if (laserbase.position.X < laserbase.rect.Width / 2)
                {
                    laserbase.position.X = laserbase.rect.Width / 2;
                    laserbase.velocity.X = 0;
                }
                laserbase.rect.X = (int)laserbase.position.X;
                laserbase.rect.Y = (int)laserbase.position.Y;

                laserbase.bbox = new BoundingBox(new Vector3(laserbase.position.X - laserbase.rect.Width / 2, laserbase.position.Y - laserbase.rect.Height / 2, 0),
                    new Vector3(laserbase.position.X + laserbase.rect.Width / 2, laserbase.position.Y + laserbase.rect.Height / 2, 0));
                if (pad[0].Buttons.A == ButtonState.Pressed && !goodlaser.visible)
                {
                    goodlaser.visible = true;
                    goodlaser.position = laserbase.position;
                    goodlasersound.Play();
                }
                if (goodlaser.visible)
                {
                    goodlaser.position += goodlaser.velocity;
                    goodlaser.rect.Y = (int)goodlaser.position.Y;
                    goodlaser.rect.X = (int)goodlaser.position.X;

                    goodlaser.bbox = new BoundingBox(new Vector3(goodlaser.position.X - goodlaser.rect.Width / 2, goodlaser.position.Y - goodlaser.rect.Height / 2, 0),
                        new Vector3(goodlaser.position.X + goodlaser.rect.Width, goodlaser.position.Y + goodlaser.rect.Height / 2, 0));

                    if (goodlaser.position.Y + goodlaser.rect.Height / 2 < 0)
                        goodlaser.visible = false;
                }

                lasercounter -= gtime;
                Boolean hitside = false;

                for (int x = 0; x < columns; x++)
                    for (int y = 0; y < rows; y++)
                    {
                        invader[x, y].velocity *= 1.0001f;

                        if (invader[x, y].visible)
                        {
                            invader[x, y].position += invader[x, y].velocity;

                            if (invader[x, y].position.X - invader[x, y].rect.Width / 2 <= 0 ||
                                invader[x, y].position.X + invader[x, y].rect.Width / 2 >= displaywidth)
                                hitside = true;

                            if (lasercounter < 0 && (invader[x, y].position.X - invader[x, y].rect.Width / 2 < laserbase.position.X) &&
                                (invader[x, y].position.X + invader[x, y].rect.Width / 2 > laserbase.position.X))
                            {
                                for(int i=0; i < badlaser.Count();i ++)
                                    if (!badlaser[i].visible)
                                    {
                                        badlasersound.Play();
                                        badlaser[i].visible = true;
                                        badlaser[i].position = invader[x, y].position;
                                        lasercounter = 600;
                                        break;
                                    }
                            }

                            
                        }
                    }

                
                for (int i = 0; i < badlaser.Count(); i++)
                    if (badlaser[i].visible)
                    {
                        badlaser[i].position += badlaser[i].velocity;
                        badlaser[i].rect.X = (int)badlaser[i].position.X;
                        badlaser[i].rect.Y = (int)badlaser[i].position.Y;
                        badlaser[i].bbox = new BoundingBox(new Vector3(badlaser[i].position.X - badlaser[i].rect.Width / 2, badlaser[i].position.Y - badlaser[i].rect.Height / 2, 0),
                            new Vector3(badlaser[i].position.X + badlaser[i].rect.Width / 2, badlaser[i].position.Y + badlaser[i].rect.Height / 2, 0));

                        if (badlaser[i].bbox.Intersects(laserbase.bbox))
                        {
                            badlaser[i].visible = false;
                            lives--;
                            goodhit.Play();
                            if (lives <= 0)
                            {
                                gameover = true;
                                gameoversound.Play();

                                if (score > highscores[numberofhighscores - 1])
                                {
                                    highscores[numberofhighscores - 1] = score;
                                    Array.Sort(highscores);
                                    Array.Reverse(highscores);
                                }
                            }
                          
                        }
                        if (badlaser[i].position.Y - badlaser[i].rect.Height / 2 > displayheight)
                            badlaser[i].visible = false;
                        
                    }

                for (int i = 0; i < badlaser.Count(); i++)
                    for (int s = 0; s < sheild.Count(); s++)
                    {
                        if (badlaser[i].visible && badlaser[i].bbox.Intersects(sheild[s].bsphere) && sheild[s].visible)
                        {
                            sheild[s].sheildlives--;
                            badlaser[i].visible = false;
                        }
                        if (sheild[s].sheildlives <= 0)
                            sheild[s].visible = false;
                    }
                
              


                for (int y = rows - 1; y >= 0; y--)
                    for (int x = 0; x < columns; x++)
                    {
                        if (hitside)
                        {


                            invader[x, y].position.Y += invader[x, y].power * 3;
                            invader[x, y].velocity.X = -invader[x, y].velocity.X;


                        }
                        invader[x, y].rect.X = (int)invader[x, y].position.X;
                        invader[x, y].rect.Y = (int)invader[x, y].position.Y;

                        invader[x, y].bbox = new BoundingBox(new Vector3(invader[x, y].position.X - invader[x, y].rect.Width / 2, invader[x, y].position.Y - invader[x, y].rect.Height / 2, 0),
                    new Vector3(invader[x, y].position.X + invader[x, y].rect.Width / 2, invader[x, y].position.Y + invader[x, y].rect.Height / 2, 0));
                        if(goodlaser.visible && invader[x,y].visible && goodlaser.bbox.Intersects(invader[x,y].bbox))
                        {
                            goodlaser.visible=false;
                            invader[x,y].visible=false;
                            score +=10;
                            invadercounter--;
                        }
                    }
                if (invadercounter <= 0)
                {
                    score += 50;
                    resetlevel();
                }


            }

            else
            {
                // Game is Over
                if (pad[0].Buttons.Back == ButtonState.Pressed)
                    gamestate = -1;
            }
        }

        public void drawgame()
        {
            // Draw the in-game graphics
            spriteBatch.Begin();

            spriteBatch.Draw(background.image, background.rect, Color.White);

            spriteBatch.DrawString(mainfont, "Score " + score.ToString() + " High Score " + highscores[0].ToString(), new Vector2(50, 10), Color.White, 0, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);

            spriteBatch.DrawString(mainfont, " Lives " + lives.ToString(), new Vector2(displaywidth - 250, 10),
                Color.White, 0, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);

            for (int i = 0; i < badlaser.Count(); i++)
            {
                if (badlaser[i].visible)
                    spriteBatch.Draw(badlaser[i].image, badlaser[i].rect, null, Color.White, 0, badlaser[i].origin, SpriteEffects.None, 0);
            }

            for (int y = 0; y < rows; y++)
                for (int x = 0; x < columns; x++)
                {
                    if (invader[x, y].visible)
                        spriteBatch.Draw(invader[x, y].image, invader[x, y].rect, null, Color.White, 0, invader[x, y].origin, SpriteEffects.None, 0);

                }
            if (goodlaser.visible)
                spriteBatch.Draw(goodlaser.image, goodlaser.rect, null, Color.White, 0, goodlaser.origin, SpriteEffects.None, 0);

            spriteBatch.Draw(laserbase.image, laserbase.rect, null, Color.White, 0, laserbase.origin, SpriteEffects.None, 0);
            if (gameover)
            {
                spriteBatch.Draw(gameoverimage.image, gameoverimage.rect, Color.White);
                spriteBatch.DrawString(mainfont, " Sorry Player 1 Your Game is Over".ToString(), new Vector2(100, 100), Color.White);
            }

            for (int s = 0; s < sheild.Count(); s++)
            {
                if (sheild[s].visible)
                    spriteBatch.Draw(sheild[s].image, sheild[s].rect, null, Color.White, 0, sheild[s].origin, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }

        public void updateoptions()
        {
            // Update code for the options screen

            // Allow game to return to the main menu
            if (keys.IsKeyDown(Keys.Escape) || pad[0].Buttons.B == ButtonState.Pressed) gamestate = -1;
        }

        public void drawoptions()
        {
            // Draw graphics for OPTIONS screen
            spriteBatch.Begin();
            spriteBatch.Draw(background.image, background.rect, Color.White);
            
            // Draw mouse
            if (optionselected > -1)
            {
                mousepointer2.rect = mousepointer1.rect;
                spriteBatch.Draw(mousepointer2.image, mousepointer2.rect, null, Color.White, mousepointer2.rotation, mousepointer2.origin, SpriteEffects.None, 0);
            }
            else
                spriteBatch.Draw(mousepointer1.image, mousepointer1.rect, null, Color.White, mousepointer1.rotation, mousepointer1.origin, SpriteEffects.None, 0);

            spriteBatch.End();
        }

        public void updatehighscore()
        {
            // Update code for the high score screen
            Array.Sort(highscores);
            Array.Reverse(highscores);
 
            // Allow game to return to the main menu
            if (keys.IsKeyDown(Keys.Escape) || pad[0].Buttons.B == ButtonState.Pressed) gamestate = -1;
        }

        public void drawhighscore()
        {
            // Draw graphics for High Score table
            spriteBatch.Begin();
            spriteBatch.Draw(background.image, background.rect, Color.White);

            // Draw top ten high scores
            for (int i = 0; i < numberofhighscores; i++)
            {
                spriteBatch.DrawString(mainfont, (i+1).ToString("0")+". "+ highscores[i].ToString("0"), new Vector2(displaywidth/2 - 30, 100+(i*30)),
                    Color.White, MathHelper.ToRadians(0), new Vector2(0, 0), 1f, SpriteEffects.None, 0);
            }

            // Draw mouse
            if (optionselected > -1)
            {
                mousepointer2.rect = mousepointer1.rect;
                spriteBatch.Draw(mousepointer2.image, mousepointer2.rect, null, Color.White, mousepointer2.rotation, mousepointer2.origin, SpriteEffects.None, 0);
            }
            else
                spriteBatch.Draw(mousepointer1.image, mousepointer1.rect, null, Color.White, mousepointer1.rotation, mousepointer1.origin, SpriteEffects.None, 0);
 
            spriteBatch.End();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            // Draw stuff depending on the game state
            switch (gamestate)
            {
                case -1:
                    // Game is on the main menu
                    drawmenu();
                    break;
                case 0:
                    // Game is being played
                    drawgame();

                    break;
                case 1:
                    // Options menu
                    drawoptions();
                    break;
                case 2:
                    // High Score table
                    drawhighscore();
                    break;
                default:
                    break;
            }

            base.Draw(gameTime);
        }
    }
}
