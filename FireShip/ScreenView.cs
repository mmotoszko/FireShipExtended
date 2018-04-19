using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Drawing;

namespace FireShip
{
    public partial class ScreenView : Form
    {
        public enum objectName { EBossShip, EPlayerShip, ENormalEnemyShip, EStrongEnemyShip, ESceneryObject, ESceneryBackground, EPowerUp, EEnemyBullet, EPlayerBullet }
        public enum bossStates { EBossEntry, EBossDeath, EBossDefense, EBossBreak }
        public static Random rnd = new Random();
        public static Stopwatch stopWatch = new Stopwatch();
        public static Stopwatch gameTime = new Stopwatch();
        public static int framesPerSecondTop = 50;
        public static int frameTime = 1000 / framesPerSecondTop;
        public static int screenHeight = 80 * 12;
        public static int screenWidth = 80 * 8;
        public static Menu menu = null;
        public static MovementStrategy MSRandom = new MovementStrategyRandom();
        public static MovementStrategy MSStraight = new MovementStrategyStraight();
        public static MovementStrategy MSWave = new MovementStrategyWave();
        public static Dictionary<objectName, Bitmap> objectSkins = new Dictionary<objectName, Bitmap>();
        public static List<MovingObject> playerShipList = new List<MovingObject>();
        public static List<MovingObject> enemyShipList = new List<MovingObject>();
        public static List<MovingObject> sceneryObjectList = new List<MovingObject>();
        public static PowerUp powerUp = null;
        public static Boss boss = null;
        public static List<string> highscoresNames = new List<string>();
        public static List<int> highscoresScores = new List<int>();
        public static string newName = null;
        public static bool highscoresOn = false;
        public static bool playerDead = false;
        public static bool gameStart = false;
        public static bool gameRunning = true;
        public static bool continueGame = true;
        public static bool drawing = false;
        public static bool godMode = false;  // for testing only
        public static bool normalEnemies = true;
        public static int difficultyTimeIncrement = 15; // 15
        public static EnemyShipFactory enemyShipFactory = EnemyShipFactory.GetEnemyShipFactory(EnemyFactoryType.NORMAL, difficultyTimeIncrement);
        public static int score;
        public static int timeScore;
        Label frameCounterLabel = new Label();
        Label scoreLabel = new Label();
        public List<Label> menuIcons;
        public List<Label> highscoresIcons;
        public PictureBox titleImage;
        public static int frameCounter = 0;
        public static int sleepTime = 0;
        public static int maxEnemiesCount = 0;
        public static int playerMoveX = 0;
        public static int playerMoveY = 0;
        public static int backgroundMoved = screenHeight * 2;
        public static bool playerFire = false;
        public object lockXMovement = new object();
        public object lockYMovement = new object();
        public object lockEnemyShipList = new object();
        public object lockPlayerShipList = new object();
        public object lockSceneryObjectList = new object();

        // Calculate enemy units' speed and position based on current speed and position
        void calculateEnemyShipsMovement()
        {
            int iCount = enemyShipList.Count;

            for (int i = 0; i < iCount; i++)
            {
                ((AutomatedMovingObject)enemyShipList[i]).move();

                if ((enemyShipList[i].getPosition().Y > 1 + enemyShipList[i].getSize() || enemyShipList[i].getPosition().Y < 0 - enemyShipList[i].getSize()))
                {
                    enemyShipList.RemoveAt(i);
                    i--;
                    iCount--;
                }
            }
            
            if (boss != null)
            {
                boss.move();
                if (boss.getPosition().Y < 0 - boss.getSize())
                    boss = null;
            }

        }

        // Calculate player's units' speed and position based on current speed and position
        void calculatePlayerShipMovement()
        {
            IPlayerShip mObj = (IPlayerShip)playerShipList[0];

            if (playerMoveX == -1)
                mObj.moveLeft();
            else if (playerMoveX == 1)
                mObj.moveRight();
            if (playerMoveY == -1)
                mObj.moveUp();
            else if (playerMoveY == 1)
                mObj.moveDown();

            double x = mObj.getPosition().X;
            double y = mObj.getPosition().Y;
            double xVel = mObj.getVelocity().X;
            double yVel = mObj.getVelocity().Y;
            double speed = mObj.getSpeed();

            mObj.setPreviousPosition(x, y);

            double newX = x + xVel;
            double newY = y + yVel;

            mObj.setVelocity(xVel * 0.9, yVel * 0.9);

            if (xVel < 0.002 && xVel > -0.002)
                xVel = 0;
            if (yVel < 0.002 && yVel > -0.002)
                yVel = 0;

            if (newX < 0)
                newX = 0;
            else if (newX > 1 - mObj.getSize() / 2)
                newX = 1 - mObj.getSize() / 2;
            if (newY < 0)
                newY = 0;
            else if (newY > 1 - mObj.getSize() / 2)
                newY = 1 - mObj.getSize() / 2;

            mObj.setPosition(newX, newY);

            int iCount = playerShipList.Count;

            for (int i = 1; i < iCount; i++)
            {
                ((AutomatedMovingObject)playerShipList[i]).move();

                if ((playerShipList[i].getPosition().Y > 1 + playerShipList[i].getSize() || playerShipList[i].getPosition().Y < 0 - playerShipList[i].getSize()) ||
                    (playerShipList[i].getPosition().X > 1 - playerShipList[i].getSize() || playerShipList[i].getPosition().X < 0 - playerShipList[i].getSize()))
                {
                    playerShipList.RemoveAt(i);
                    i--;
                    iCount--;
                }
            }

            if (powerUp != null)
            {
                powerUp.move();

                if ((powerUp.getPosition().Y > 1 + powerUp.getSize() || powerUp.getPosition().Y < 0 - powerUp.getSize()) ||
                    (powerUp.getPosition().X > 1 - powerUp.getSize() || powerUp.getPosition().X < 0 - powerUp.getSize()))
                    powerUp = null;
            }
        }

        // Calculate scenery objects' speed and position based on current speed and position
        void calculateSceneryMovement()
        {
            int iCount = sceneryObjectList.Count;

            for (int i = 0; i < iCount; i++)
            {
                ((AutomatedMovingObject)sceneryObjectList[i]).move();

                if ((sceneryObjectList[i].getPosition().Y > 1 + sceneryObjectList[i].getSize() || sceneryObjectList[i].getPosition().Y < 0 - sceneryObjectList[i].getSize()) ||
                    (sceneryObjectList[i].getPosition().X > 1 - sceneryObjectList[i].getSize() || sceneryObjectList[i].getPosition().X < 0 - sceneryObjectList[i].getSize()))
                {
                    sceneryObjectList.RemoveAt(i);
                    i--;
                    iCount--;
                }
            }
        }

        // Drawing event - this is where all moving objects are processed into a buffer which is then drawn in the main window
        private void ScreenView_Paint(object sender, PaintEventArgs e)
        {
            Bitmap bmpBuffer = new Bitmap(screenWidth, screenHeight);
            Brush brush = new SolidBrush(Color.FromArgb(0, 0, 0, 0));

            Graphics G = Graphics.FromImage(bmpBuffer);
            Font enemyFont = new Font("Consolas", 8f);
            Font powerUpFont = new Font("Consolas", 11f);
            Font bossFont = new Font("Consolas", 16f);

            // Draw pre-rendered background scenery
            Bitmap backgroundImg;
            objectSkins.TryGetValue(objectName.ESceneryBackground, out backgroundImg);
            Rectangle destRect = new Rectangle(0, 0, screenWidth, screenHeight);
            Rectangle srcRect = new Rectangle(0, backgroundMoved, screenWidth, screenHeight);
            e.Graphics.DrawImage(backgroundImg, destRect, srcRect, GraphicsUnit.Pixel);

            // Draw background scenery
            foreach (SceneryObject obj in sceneryObjectList)
            {
                if (obj.getDepth() >= 0)
                {
                    double size = obj.getSize();
                    Rectangle rect = new Rectangle((int)(obj.getPosition().X * screenWidth - screenWidth * size / 2), (int)(obj.getPosition().Y * screenHeight - screenHeight * size / 2), (int)(size * screenWidth), (int)(size * screenHeight));
                    Bitmap img;
                    objectSkins.TryGetValue(objectName.ESceneryObject, out img);
                    G.DrawImage(img, rect);
                }
            }

            // Draw player units - ship, bullets
            foreach (MovingObject obj in playerShipList)
            {
                double size = obj.getSize();

                if (obj.getName() == objectName.EPlayerBullet)
                {
                    float angle = 0;
                    int x = (int)(obj.getPosition().X * screenWidth - screenWidth * size / 2);
                    int y = (int)(obj.getPosition().Y * screenHeight - screenHeight * size / 2);
                    switch (((Bullet)obj).getShape())
                    {
                        case '1':
                            break;
                        case '2':
                            angle = -30.0F;
                            break;
                        case '3':
                            angle = 30.0F;
                            break;
                        case '4':
                            angle = 90.0F;
                            x += 10;
                            break;
                        case '5':
                            angle = -90.0F;
                            y += 7;
                            break;
                    }

                    Rectangle rect = new Rectangle(0, 0, (int)(size * screenWidth), (int)(size * screenHeight));
                    Bitmap img;

                    objectSkins.TryGetValue(objectName.EPlayerBullet, out img);
                    G.TranslateTransform(x, y);
                    G.RotateTransform(angle);
                    G.DrawImage(img, rect);
                    G.RotateTransform(-angle);
                    G.TranslateTransform(-x, -y);
                }
                else
                {
                    double playerSize = PlayerShip.getInstance().getSize();
                    Rectangle playerRect = new Rectangle((int)(PlayerShip.getInstance().getPosition().X * screenWidth - screenWidth * playerSize / 2), (int)(PlayerShip.getInstance().getPosition().Y * screenHeight - screenHeight * playerSize / 2), (int)(playerSize * screenWidth), (int)(playerSize * screenHeight));
                    Bitmap playerImg;
                    objectSkins.TryGetValue(objectName.EPlayerShip, out playerImg);
                    if (godMode)
                        playerImg.SetPixel(0, 0, Color.White);
                    else
                        playerImg.SetPixel(0, 0, Color.Transparent);
                    G.DrawImage(playerImg, playerRect);
                }
            }

            // Draw powerup
            if (powerUp != null)
            {
                double powerUpSize = powerUp.getSize();
                Rectangle powerUpRect = new Rectangle((int)(powerUp.getPosition().X * screenWidth - screenWidth * powerUpSize / 2), (int)(powerUp.getPosition().Y * screenHeight - screenHeight * powerUpSize / 2), (int)(powerUpSize * screenWidth), (int)(powerUpSize * screenHeight));
                Bitmap powerUpImg;
                objectSkins.TryGetValue(objectName.EPowerUp, out powerUpImg);
                G.DrawImage(powerUpImg, powerUpRect);
                G.DrawString(powerUp.getShape().ToString(), powerUpFont, Brushes.LawnGreen, (int)(powerUp.getPosition().X * screenWidth) - 6, (int)(powerUp.getPosition().Y * screenHeight) - 8);
            }

            // Draw enemy units - ships, bullets
            foreach (MovingObject obj in enemyShipList)
            {
                double size = obj.getSize();
                Bitmap img;

                if (obj.getName() == objectName.EEnemyBullet)
                {
                    Rectangle rect = new Rectangle((int)(obj.getPosition().X * screenWidth - screenWidth * size / 2), (int)(obj.getPosition().Y * screenHeight - screenHeight * size / 2), (int)(size * screenWidth), (int)(size * screenHeight));
                    objectSkins.TryGetValue(objectName.EEnemyBullet, out img);
                    G.DrawImage(img, rect);
                }
                else
                {
                    if (obj.getName() == objectName.EStrongEnemyShip)
                        objectSkins.TryGetValue(objectName.EStrongEnemyShip, out img);
                    else
                        objectSkins.TryGetValue(objectName.ENormalEnemyShip, out img);
                    Rectangle rect = new Rectangle((int)(obj.getPosition().X * screenWidth - screenWidth * size / 2), (int)(obj.getPosition().Y * screenHeight - screenHeight * size / 2), (int)(size * screenWidth), (int)(size * screenHeight));
                    G.DrawImage(img, rect);
                    if (((EnemyShip)obj).getHitpoints() < 10)
                        G.DrawString(((EnemyShip)obj).getHitpoints().ToString(), enemyFont, Brushes.Red, (int)(obj.getPosition().X * screenWidth) - 5, (int)(obj.getPosition().Y * screenHeight) - 7);
                    else
                        G.DrawString(((EnemyShip)obj).getHitpoints().ToString(), enemyFont, Brushes.Red, (int)(obj.getPosition().X * screenWidth) - 9, (int)(obj.getPosition().Y * screenHeight) - 7);
                }

            }

            //Draw Boss
            if (boss != null)
            {
                double bossSize = boss.getSize();
                Rectangle bossRect = new Rectangle((int)(boss.getPosition().X * screenWidth - screenWidth * bossSize / 2), (int)(boss.getPosition().Y * screenHeight - screenHeight * bossSize / 2), (int)(bossSize * screenWidth), (int)(bossSize * screenHeight));
                Bitmap bossImg;
                objectSkins.TryGetValue(objectName.EBossShip, out bossImg);
                G.DrawImage(bossImg, bossRect);
                if (boss.stateName == bossStates.EBossDefense)
                {
                    G.DrawString(boss.getDefence().ToString(), bossFont, Brushes.Gray, (int)(boss.getPosition().X * screenWidth) - 9, (int)(boss.getPosition().Y * screenHeight) - 15);
                }
                if (boss.stateName == bossStates.EBossBreak)
                {
                    if (boss.getHitpoints() < 10)
                        G.DrawString(boss.getHitpoints().ToString(), bossFont, Brushes.Red, (int)(boss.getPosition().X * screenWidth) - 9, (int)(boss.getPosition().Y * screenHeight) - 15);
                    else
                        G.DrawString(boss.getHitpoints().ToString(), bossFont, Brushes.Red, (int)(boss.getPosition().X * screenWidth) - 13, (int)(boss.getPosition().Y * screenHeight) - 15);
                }
            }

            // Draw foreground scenery
            foreach (SceneryObject obj in sceneryObjectList)
            {
                if (obj.getDepth() < 0)
                {
                    double size = obj.getSize();
                    Rectangle rect = new Rectangle((int)(obj.getPosition().X * screenWidth - screenWidth * size / 2), (int)(obj.getPosition().Y * screenHeight - screenHeight * size / 2), (int)(size * screenWidth), (int)(size * screenHeight));
                    Bitmap img;
                    objectSkins.TryGetValue(objectName.ESceneryObject, out img);
                    G.DrawImage(img, rect);
                }
            }

            // Draw buffer on screen
            e.Graphics.DrawImage(bmpBuffer, new PointF(0.0F, 0.0F));

            G.Dispose();
            enemyFont.Dispose();
            powerUpFont.Dispose();
            bmpBuffer.Dispose();

            drawing = false;
        }

        // Create bitmap from text, needed for indicating that can't load bitmaps without stopping the program
        private Image DrawText(string text, Font font, Color textColor, Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;

        }

        void upgradePlayerShip(char c)
        {
            IPlayerShip tmp = (IPlayerShip)playerShipList[0];
            switch (c)
            {
                case 'P':
                    tmp = new PlayerShipPowerPowerUp(tmp);
                    break;
                case 'F':
                    tmp = new PlayerShipFireRatePowerUp(tmp);
                    break;
                case 'S':
                    tmp = new PlayerShipSpeedPowerUp(tmp);
                    break;
                case 'M':
                    tmp = new PlayerShipMultipleBulletsPowerUp(tmp);
                    break;
                default: break;
            }
            playerShipList[0] = (MovingObject)tmp;
        }

        // Check for collisions between units that can collide, hitboxes are circles centered on the center of an object with the radius of the object's size / 2
        void checkCollisions()
        {
            int iCount = playerShipList.Count;
            int jCount = enemyShipList.Count;

            if (powerUp != null)
                if (powerUp.getPosition().Y > 1)
                    powerUp = null;

            for (int i = 0; i < iCount; i++)
            {
                if (powerUp != null && i == 0)
                {
                    double size1 = ((IPlayerShip)playerShipList[i]).getSize();
                    double size2 = powerUp.getSize();
                    double pos1X = ((IPlayerShip)playerShipList[i]).getPosition().X;
                    double pos1Y = ((IPlayerShip)playerShipList[i]).getPosition().Y;
                    double pos2X = powerUp.getPosition().X;
                    double pos2Y = powerUp.getPosition().Y;

                    if ((pos1X - pos2X) * (pos1X - pos2X) + (pos1Y - pos2Y) * (pos1Y - pos2Y) < (size1 / 2 + size2 / 2) * (size1 / 2 + size2 / 2))
                    {
                        upgradePlayerShip(powerUp.getShape());
                        powerUp = null;
                    }
                }

                if (boss != null)
                {
                    double size1 = playerShipList[i].getSize();
                    double pos1X = playerShipList[i].getPosition().X;
                    double pos1Y = playerShipList[i].getPosition().Y;
                    if (i == 0)
                    {
                        size1 = ((IPlayerShip)playerShipList[i]).getSize();
                        pos1X = ((IPlayerShip)playerShipList[i]).getPosition().X;
                        pos1Y = ((IPlayerShip)playerShipList[i]).getPosition().Y;
                    }
                    double size2 = boss.getSize();
                    double pos2X = boss.getPosition().X;
                    double pos2Y = boss.getPosition().Y;

                    if ((pos1X - pos2X) * (pos1X - pos2X) + (pos1Y - pos2Y) * (pos1Y - pos2Y) < (size1 / 2 + size2 / 2) * (size1 / 2 + size2 / 2))
                    {
                        // If boss collided with player
                        if (i == 0)
                        {
                            if (!godMode)
                                playerDead = true;
                        }
                        // If player bullet hit boss
                        else
                        {
                            boss.defend(((Bullet)playerShipList[i]).getDamage());
                            playerShipList.RemoveAt(i);
                            i--;
                            iCount--;
                        }
                    }

                }


                for (int j = 0; j < jCount; j++)
                {
                    double size1 = playerShipList[i].getSize();
                    double pos1X = playerShipList[i].getPosition().X;
                    double pos1Y = playerShipList[i].getPosition().Y;
                    if (i == 0)
                    {
                        size1 = ((IPlayerShip)playerShipList[i]).getSize();
                        pos1X = ((IPlayerShip)playerShipList[i]).getPosition().X;
                        pos1Y = ((IPlayerShip)playerShipList[i]).getPosition().Y;
                    }
                    double size2 = enemyShipList[j].getSize();
                    double pos2X = enemyShipList[j].getPosition().X;
                    double pos2Y = enemyShipList[j].getPosition().Y;

                    if ((pos1X - pos2X) * (pos1X - pos2X) + (pos1Y - pos2Y) * (pos1Y - pos2Y) < (size1 / 2 + size2 / 2) * (size1 / 2 + size2 / 2))
                    {
                        if (i == 0 && !godMode)
                        {
                            playerDead = true;
                        }
                        else
                        {
                            if (enemyShipList[j].getName() == objectName.ENormalEnemyShip && playerShipList[i].getName() == objectName.EPlayerBullet)
                            {
                                if (((EnemyShip)enemyShipList[j]).takeDamage(((Bullet)playerShipList[i]).getDamage()) <= 0)
                                {
                                    score += 5;
                                    enemyShipList.RemoveAt(j);
                                    j--;
                                    jCount--;
                                }
                            }
                            else if (enemyShipList[j].getName() == objectName.EStrongEnemyShip && playerShipList[i].getName() == objectName.EPlayerBullet)
                            {
                                if (((EnemyShip)enemyShipList[j]).takeDamage(((Bullet)playerShipList[i]).getDamage()) <= 0)
                                {
                                    score += 25;
                                    enemyShipList.RemoveAt(j);
                                    j--;
                                    jCount--;
                                }
                            }
                            else if (enemyShipList[j].getName() == objectName.EEnemyBullet)
                            {
                                enemyShipList.RemoveAt(j);
                                j--;
                                jCount--;
                            }

                            if (!godMode || playerShipList[i].getName() == objectName.EPlayerBullet)
                            {
                                playerShipList.RemoveAt(i);
                                i--;
                                iCount--;
                            }
                        }
                    }
                }
            }
        }

        // Set values to variables needed to start a new game
        void initializeGame()
        {
            enemyShipList.Clear();
            playerShipList.Clear();

            gameStart = false;
            playerDead = false;
            newName = null;
            boss = null;
            powerUp = null;
            continueGame = true;
            score = 0;
            playerMoveY = 0;
            playerMoveX = -5;

            PlayerShip s = PlayerShip.getReset();
            playerShipList.Add(s);
        }

        // Read highscores from file
        static void readHighscores(List<string> listNames, List<int> listScores)
        {
            string[] highscores;

            if (File.Exists("highscores.txt"))
                highscores = File.ReadAllLines("highscores.txt");
            else
            {
                var fileHighscores = File.Create("highscores.txt");
                fileHighscores.Close();
                return;
            }

            for (int i = 0; i < highscores.Count(); i++)
            {
                int tempInt;
                string[] tempString = highscores[i].Split(null);
                int.TryParse(tempString[0], out tempInt);
                listScores.Add(tempInt);
                string fullName = "";
                for (int j = 1; j < tempString.Count(); j++)
                {
                    if (j > 1)
                        fullName += " ";
                    fullName += tempString[j];
                }
                listNames.Add(fullName);
            }
        }

        // Save highscores to file
        static void saveHighscores(List<string> listNames, List<int> listScores)
        {
            string[] highscores = new string[listNames.Count];

            for (int i = 0; i < listNames.Count(); i++)
            {
                string tempString = listScores[i] + " " + listNames[i];
                highscores[i] = tempString;
            }

            var fileHighscores = File.Create("highscores.txt");
            fileHighscores.Close();
            File.WriteAllLines("highscores.txt", highscores);
        }

        // Return true if given value is higher than any of currently saved highscores
        static bool checkHighscores(int value)
        {
            if (highscoresScores.Count < 9)
                return true;

            for (int i = 0; i < highscoresScores.Count(); i++)
            {
                if (value > highscoresScores[i])
                    return true;
            }

            return false;
        }

        // Add highscore with name to the highscores list
        static void addHighscore(int value, string name)
        {
            for (int i = 0; i < highscoresScores.Count(); i++)
            {
                if (value > highscoresScores[i])
                {
                    highscoresScores.Insert(i, value);
                    highscoresNames.Insert(i, name);

                    if (highscoresScores.Count > 9)
                    {
                        highscoresScores.RemoveAt(9);
                        highscoresNames.RemoveAt(9);
                    }

                    return;
                }
            }

            highscoresScores.Add(value);
            highscoresNames.Add(name);
        }

        // Hide menu controls and show highscores list
        void showHighscores()
        {
            highscoresIcons = new List<Label>();

            Label hLabel = new Label();
            hLabel.BackColor = Color.Black;
            hLabel.ForeColor = Color.LightGray;
            hLabel.TextAlign = ContentAlignment.MiddleCenter;
            hLabel.Text = "H i g h s c o r e s";
            hLabel.AutoSize = true;
            hLabel.Font = new Font("Arial", 10);
            hLabel.Top = screenHeight / 3 - 40;
            hLabel.Refresh();
            highscoresIcons.Add(hLabel);
            try
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    Controls.Add(hLabel);
                    hLabel.BringToFront();
                    hLabel.Left = screenWidth / 2 - hLabel.Width / 2;
                }));
            }
            catch { };

            int i;

            for (i = 0; i < highscoresNames.Count; i++)
            {
                hLabel = new Label();
                hLabel.BackColor = Color.Black;
                hLabel.ForeColor = Color.LightGray;
                hLabel.TextAlign = ContentAlignment.MiddleLeft;
                hLabel.Text = (i + 1).ToString() + ".  " + highscoresNames[i];
                hLabel.AutoSize = true;
                hLabel.Font = new Font("Arial", 10);
                hLabel.Left = screenWidth / 4;// - 100;
                hLabel.Top = screenHeight / 3 + 25 * i;
                hLabel.Refresh();
                highscoresIcons.Add(hLabel);
                try
                {
                    Invoke(new MethodInvoker(delegate ()
                    {
                        Controls.Add(hLabel);
                        hLabel.BringToFront();
                    }));
                }
                catch { };
            }

            for (i = 0; i < highscoresScores.Count; i++)
            {
                hLabel = new Label();
                hLabel.BackColor = Color.Black;
                hLabel.ForeColor = Color.LightGray;
                hLabel.TextAlign = ContentAlignment.MiddleRight;
                hLabel.Text = highscoresScores[i].ToString();
                hLabel.AutoSize = true;
                hLabel.Font = new Font("Arial", 10);
                hLabel.Refresh();
                highscoresIcons.Add(hLabel);
                try
                {
                    Invoke(new MethodInvoker(delegate ()
                    {
                        Controls.Add(hLabel);
                        hLabel.BringToFront();
                        hLabel.Left = screenWidth * 3 / 4 - hLabel.Width;// + 110// - hLabel.Width;
                        hLabel.Top = screenHeight / 3 + 25 * i;
                    }));
                }
                catch { };
            }

            hLabel = new Label();
            hLabel.BackColor = Color.Black;
            hLabel.ForeColor = Color.LightGray;
            hLabel.TextAlign = ContentAlignment.MiddleCenter;
            hLabel.Text = "B a c k";
            hLabel.AutoSize = true;
            hLabel.Font = new Font("Arial", 10);
            hLabel.Top = screenHeight / 3 + 25 * 9 + 50;
            hLabel.Refresh();
            highscoresIcons.Add(hLabel);
            try
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    Controls.Add(hLabel);
                    hLabel.BringToFront();
                    hLabel.Left = screenWidth / 2 - hLabel.Width / 2;
                }));
            }
            catch { };

        }

        // Hide highscores list and show menu controls
        void removeHighscores()
        {
            for (int i = 0; i < highscoresIcons.Count; i++)
            {
                try
                {
                    Invoke(new MethodInvoker(delegate ()
                    {
                        Controls.Remove(highscoresIcons[i]);
                    }));
                }
                catch { };
            }

            highscoresIcons.Clear();
        }

        // Show game over screen
        void showGameOver()
        {
            Label gOContinue = new Label();
            gOContinue.BackColor = Color.Black;
            gOContinue.ForeColor = Color.LightGray;
            gOContinue.AutoSize = true;
            gOContinue.Top = screenHeight / 3 + 25 * 9 + 50;
            gOContinue.Text = "C o n t i n u e";
            gOContinue.Font = new Font("Arial", 10);

            Label gOGameOver = new Label();
            gOGameOver.BackColor = Color.Black;
            gOGameOver.ForeColor = Color.LightGray;
            gOGameOver.AutoSize = true;
            gOGameOver.Top = screenHeight / 3;
            gOGameOver.Text = "G a m e   O v e r";
            gOGameOver.Font = new Font("Arial", 10);

            Label gOScore = new Label();
            gOScore.BackColor = Color.Black;
            gOScore.ForeColor = Color.LightGray;
            gOScore.AutoSize = true;
            gOScore.Top = screenHeight / 3 + 46;
            gOScore.Text = "Score:   " + (score + timeScore).ToString();
            gOScore.Font = new Font("Arial", 10);

            try
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    Controls.Add(gOContinue);
                    gOContinue.BringToFront();
                    gOContinue.Left = screenWidth / 2 - gOContinue.Width / 2 + 5;

                    Controls.Add(gOGameOver);
                    gOGameOver.BringToFront();
                    gOGameOver.Left = screenWidth / 2 - gOGameOver.Width / 2 + 5;

                    Controls.Add(gOScore);
                    gOScore.BringToFront();
                    gOScore.Left = screenWidth / 2 - gOScore.Width / 2 + 5;

                    for (int i = menuIcons.Count - 2; i <= menuIcons.Count - 1; i++)
                    {
                        menuIcons[i].Top = screenHeight / 3 + 25 * 9 + 50;
                        menuIcons[i].Visible = true;
                    }
                }));
            }
            catch { };
        }

        // Hide game over screen
        void removeGameOver()
        {
            try
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    for (int i = 0; i < 3; i++)
                        Controls.RemoveAt(0);
                }));
            }
            catch { };
        }

        // Show game over screen with new highscore achieved
        void showNewHighscore()
        {
            Label gOContinue = new Label();
            gOContinue.BackColor = Color.Black;
            gOContinue.ForeColor = Color.LightGray;
            gOContinue.AutoSize = true;
            gOContinue.Top = screenHeight / 3 + 25 * 9 + 50;
            gOContinue.Text = "C o n t i n u e";
            gOContinue.Font = new Font("Arial", 10);

            Label gOGameOver = new Label();
            gOGameOver.BackColor = Color.Black;
            gOGameOver.ForeColor = Color.LightGray;
            gOGameOver.AutoSize = true;
            gOGameOver.Top = screenHeight / 3;
            gOGameOver.Text = "N e w   H i g h s c o r e !";
            gOGameOver.Font = new Font("Arial", 10);

            Label gOScore = new Label();
            gOScore.BackColor = Color.Black;
            gOScore.ForeColor = Color.LightGray;
            gOScore.AutoSize = true;
            gOScore.Top = screenHeight / 3 + 46;
            gOScore.Text = "Score:   " + (score + timeScore).ToString();
            gOScore.Font = new Font("Arial", 10);

            Label gOName = new Label();
            gOName.BackColor = Color.Black;
            gOName.ForeColor = Color.LightGray;
            gOName.AutoSize = true;
            gOName.Top = screenHeight / 3 + 80;
            gOName.Text = "Your name:";
            gOName.Font = new Font("Arial", 10);

            TextBox gOIn = new TextBox();
            gOIn.BackColor = Color.Black;
            gOIn.ForeColor = Color.LightGray;
            gOIn.BorderStyle = BorderStyle.None;
            gOIn.Height = 20;
            gOIn.Width = 300;
            gOIn.TextAlign = HorizontalAlignment.Center;
            gOIn.MaxLength = 20;
            gOIn.Top = screenHeight / 3 + 110;
            gOIn.Font = new Font("Arial", 10);
            gOIn.KeyDown += NameEntered;

            try
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    Controls.Add(gOContinue);
                    gOContinue.BringToFront();
                    gOContinue.Left = screenWidth / 2 - gOContinue.Width / 2 + 5;

                    Controls.Add(gOGameOver);
                    gOGameOver.BringToFront();
                    gOGameOver.Left = screenWidth / 2 - gOGameOver.Width / 2 + 5;

                    Controls.Add(gOScore);
                    gOScore.BringToFront();
                    gOScore.Left = screenWidth / 2 - gOScore.Width / 2 + 5;

                    Controls.Add(gOName);
                    gOName.BringToFront();
                    gOName.Left = screenWidth / 2 - gOName.Width / 2 + 5;

                    Controls.Add(gOIn);
                    gOIn.BringToFront();
                    gOIn.Left = screenWidth / 2 - gOIn.Width / 2 + 5;
                    gOIn.Focus();

                    for (int i = menuIcons.Count - 2; i <= menuIcons.Count - 1; i++)
                    {
                        menuIcons[i].Top = screenHeight / 3 + 25 * 9 + 50;
                        menuIcons[i].Visible = true;
                    }
                }));
            }
            catch { };
        }

        // Hige new highscore game over screen
        void removeNewHighscore()
        {
            try
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    for (int i = 0; i < 5; i++)
                        Controls.RemoveAt(0);
                }));
            }
            catch { };
        }

        // Initilize components, optimize drawing settings
        public ScreenView()
        {
            InitializeComponent();
            this.SetStyle(
                System.Windows.Forms.ControlStyles.UserPaint |
                System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
                System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,
                true);
        }

        // Key down event
        private void AnyKeyDown(object sender, KeyEventArgs e)
        {
            if (continueGame)
            {
                if (e.KeyCode == Keys.G)
                {
                    if (godMode)
                        godMode = false;
                    else
                        godMode = true;
                }

                if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
                {
                    lock (lockXMovement)
                    {
                        if (playerMoveX == -5)
                            playerMoveX = -1;
                        else if (playerMoveX > -1)
                            playerMoveX--;
                    }
                }
                if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
                {
                    lock (lockXMovement)
                    {
                        if (playerMoveX == -5)
                            playerMoveX = 1;
                        else if (playerMoveX < 1)
                            playerMoveX++;
                    }
                }
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
                {
                    if (!gameStart)
                    {
                        if (menu.getSelection() != 3)
                        {
                            menuIcons[menuIcons.Count - 2].Top = screenHeight * 2 / 5 + 30 * menu.selectUp();
                            menuIcons[menuIcons.Count - 1].Top = screenHeight * 2 / 5 + 30 * menu.getSelection();
                        }
                    }
                    else
                    {
                        lock (lockYMovement)
                        {
                            if (playerMoveY > -1)
                                playerMoveY--;
                        }
                    }
                }
                if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
                {
                    if (!gameStart)
                    {
                        if (menu.getSelection() != 3)
                        {
                            menuIcons[menuIcons.Count - 2].Top = screenHeight * 2 / 5 + 30 * menu.selectDown();
                            menuIcons[menuIcons.Count - 1].Top = screenHeight * 2 / 5 + 30 * menu.getSelection();
                        }
                    }
                    else
                    {
                        lock (lockYMovement)
                        {
                            if (playerMoveY < 1)
                                playerMoveY++;
                        }
                    }
                }

            }
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                if (gameStart)
                    playerFire = true;
                else
                {
                    switch (menu.getSelection())
                    {
                        case 0:
                            gameStart = true;
                            titleImage.Visible = false;
                            for (int i = 0; i < menuIcons.Count; i++)
                                menuIcons[i].Visible = false;
                            gameTime.Restart();
                            break;
                        case 1:
                            for (int i = 0; i < menuIcons.Count - 2; i++)
                                menuIcons[i].Visible = false;
                            showHighscores();
                            menuIcons[menuIcons.Count - 2].Top = screenHeight / 3 + 25 * 9 + 50;
                            menuIcons[menuIcons.Count - 1].Top = screenHeight / 3 + 25 * 9 + 50;
                            menu.setHighscoresOn();
                            break;
                        case 2:
                            gameRunning = false;
                            Close();
                            break;
                        case 3:
                            highscoresOn = false;
                            menu.setHighscoresOff();
                            removeHighscores();
                            for (int i = 0; i < menuIcons.Count - 2; i++)
                                menuIcons[i].Visible = true;
                            menuIcons[menuIcons.Count - 2].Top = screenHeight * 2 / 5 + 30 * menu.getSelection();
                            menuIcons[menuIcons.Count - 1].Top = screenHeight * 2 / 5 + 30 * menu.getSelection();
                            break;
                        default: break;
                    }
                }

                if (!continueGame)
                    continueGame = true;

            }
        }

        // Key up event
        private void AnyKeyUp(object sender, KeyEventArgs e)
        {
            if (continueGame)
            {
                if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
                {
                    lock (lockXMovement)
                    {
                        if (playerMoveX == -5)
                            playerMoveX = 0;
                        else if (playerMoveX < 1)
                            playerMoveX++;
                    }
                }
                if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
                {
                    lock (lockXMovement)
                    {
                        if (playerMoveX == -5)
                            playerMoveX = 0;
                        else if (playerMoveX > -1)
                            playerMoveX--;
                    }
                }
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
                {
                    if (gameStart)
                        lock (lockYMovement)
                        {
                            if (playerMoveY < 1)
                                playerMoveY++;
                        }
                }
                if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
                {
                    if (gameStart)
                        lock (lockYMovement)
                        {
                            if (playerMoveY > -1)
                                playerMoveY--;
                        }
                }

            }
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                playerFire = false;
            }
        }

        // Name entering event - called when player reached a new highscore and is asked to save their name
        private void NameEntered(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                newName = (sender as TextBox).Text.ToString();
            }
        }

        // All of actual game logic to be executed on a seperate thread
        private void mainLoop()
        {
            while (gameRunning)
            {
                // Start the timer for current frame
                stopWatch.Start();
                frameCounter++;

                // Invalidate form to call Paint event and redraw moved objects
                drawing = true;
                Invalidate();
                while (drawing && gameRunning)
                {

                }

                // Add new random moving scenery object
                if (frameCounter == 10)
                {
                    SceneryObject s = new SceneryObject();
                    s.setStrategy(MSStraight);
                    sceneryObjectList.Add(s);
                }

                // If player pressed 'Start'
                if (gameStart)
                {
                    // If player ship hasn't been destroyed
                    if (!playerDead)
                    {
                        // If player fired a single shot
                        if (playerFire)
                        {
                            // Create new bullets based on player's stats and position
                            ((IPlayerShip)playerShipList[0]).shoot();
                        }

                        if (frameCounter == 10 && gameTime.ElapsedMilliseconds % 2000 >= 0 && gameTime.ElapsedMilliseconds % 2000 <= 1000)
                        {
                            if (gameTime.ElapsedMilliseconds % 30000 >= 0 && gameTime.ElapsedMilliseconds % 30000 <= 1000 && normalEnemies)
                            {
                                enemyShipFactory = EnemyShipFactory.GetEnemyShipFactory(EnemyFactoryType.STRONG, difficultyTimeIncrement);
                                normalEnemies = false;
                            }

                            EnemyShip s = enemyShipFactory.CreateEnemy();

                            if (!normalEnemies)
                            {
                                enemyShipFactory = EnemyShipFactory.GetEnemyShipFactory(EnemyFactoryType.NORMAL, difficultyTimeIncrement);
                                normalEnemies = true;
                            }

                            if (enemyShipList.Count % 2 == 0)
                                s.setStrategy(MSWave);
                            else
                                s.setStrategy(MSRandom);
                            enemyShipList.Add(s);
                        }

                        // Boss shooting in broke state
                        if (frameCounter == 30 && boss != null)
                        {
                           boss.shoot();
                        }

                        if (frameCounter == 15)
                        {
                            if (boss == null)
                            {
                                if ((double)gameTime.ElapsedMilliseconds % (difficultyTimeIncrement * 1000) > difficultyTimeIncrement * 800)
                                {
                                    boss = new Boss((int)((double)gameTime.ElapsedMilliseconds / 1000 / difficultyTimeIncrement * 4), 4, 8);
                                }
                            }
                        }
                        // Enemies shooting at semi-random times
                        if (frameCounter == 20 && enemyShipList.Count != 0)
                        {
                            for (int i = 0; i < enemyShipList.Count; i++)
                            {
                                if (enemyShipList[i].getName() == objectName.ENormalEnemyShip || enemyShipList[i].getName() == objectName.EStrongEnemyShip)
                                    if (rnd.NextDouble() < (double)gameTime.ElapsedMilliseconds / (difficultyTimeIncrement * 1000) / 100)
                                        ((EnemyShip)enemyShipList[i]).shoot();
                            }
                        }

                        // If power up doesn't already exist, create a new random one
                        if (frameCounter == 25)
                        {
                            if (powerUp == null)
                            {
                                if ((double)gameTime.ElapsedMilliseconds % (difficultyTimeIncrement * 1000) > difficultyTimeIncrement * 800)
                                {
                                    powerUp = new PowerUp((IPlayerShip)playerShipList[0]);
                                    powerUp.setStrategy(MSStraight);
                                }
                            }
                        }

                        checkCollisions();
                        calculateEnemyShipsMovement();



                        // Calculate current score and update score-showing control
                        timeScore = (int)gameTime.ElapsedMilliseconds / 1000;
                        try
                        {
                            Invoke(new MethodInvoker(delegate ()
                            {
                                scoreLabel.Text = "Score: " + (score + timeScore);
                            }));
                        }
                        catch { };
                    }
                    else
                    {
                        // If player reached score high enough to save
                        if (checkHighscores(score + timeScore))
                        {
                            // Show new highscore screen and wait for name to be entered
                            continueGame = false;
                            showNewHighscore();

                            while (newName == null)
                            {
                                if (!gameRunning)
                                    break;
                            }
                            removeNewHighscore();

                            // Save given name with current highscore
                            addHighscore(score + timeScore, newName);
                            saveHighscores(highscoresNames, highscoresScores);

                            // Show menu
                            try
                            {
                                Invoke(new MethodInvoker(delegate ()
                                {
                                    titleImage.Visible = true;

                                    for (int i = 0; i < menuIcons.Count; i++)
                                        menuIcons[i].Visible = true;
                                    menuIcons[menuIcons.Count - 2].Top = screenHeight * 2 / 5 + 30 * menu.getSelection();
                                    menuIcons[menuIcons.Count - 1].Top = screenHeight * 2 / 5 + 30 * menu.getSelection();
                                }));
                            }
                            catch { };
                        }
                        else
                        {
                            // Show game over screen and wait for confirmation
                            continueGame = false;
                            showGameOver();
                            while (!continueGame)
                            {
                                if (!gameRunning)
                                    break;
                            }
                            removeGameOver();

                            // Show menu
                            try
                            {
                                Invoke(new MethodInvoker(delegate ()
                                {
                                    titleImage.Visible = true;

                                    for (int i = 0; i < menuIcons.Count; i++)
                                        menuIcons[i].Visible = true;
                                    menuIcons[menuIcons.Count - 2].Top = screenHeight * 2 / 5 + 30 * menu.getSelection();
                                    menuIcons[menuIcons.Count - 1].Top = screenHeight * 2 / 5 + 30 * menu.getSelection();
                                }));
                            }
                            catch { };
                        }

                        // Get ready for new game to be started
                        initializeGame();
                    }

                }

                // Calculate player and scenery movement during the game as well as in the menu
                if (!playerDead)
                    calculatePlayerShipMovement();
                calculateSceneryMovement();

                // Move pre-rendered background scenery
                if (frameCounter % 3 == 0)
                {
                    if (backgroundMoved == 1)
                        backgroundMoved = screenHeight * 2;
                    else
                        backgroundMoved -= 1;
                }

                // Sleep thread to achieve current fps limit
                sleepTime = frameTime - (int)stopWatch.ElapsedMilliseconds;
                if (sleepTime > 0)
                    Thread.Sleep(sleepTime);

                // Update fps-showing control
                if (frameCounter == framesPerSecondTop)
                {
                    try
                    {
                        Invoke(new MethodInvoker(delegate ()
                        {
                            frameCounter = 0;
                            frameCounterLabel.Text = "FPS: " + (1000 / stopWatch.ElapsedMilliseconds + 1) + " ";
                            frameCounterLabel.Refresh();
                        }));
                    }
                    catch { };
                }

                // Reset frame timer
                stopWatch.Reset();
            }
        }

        // Initialize the game on launching application, start new thread executing game logic - mainLoop
        private void ScreenView_Shown(object sender, EventArgs e)
        {
            initializeGame();
            Refresh();

            Thread t = new Thread(new ThreadStart(mainLoop));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        // Close application on closing the main window
        private void ScreenView_FormClosed(object sender, FormClosedEventArgs e)
        {
            gameRunning = false;
        }

        // Load textures and highscores from files, set essential controls on program start
        private void ScreenView_Load(object sender, EventArgs e)
        {
            Bitmap title;
            try
            {
                objectSkins.Add(objectName.EPlayerShip, new Bitmap("..\\..\\media\\playership.png"));
                objectSkins.Add(objectName.ENormalEnemyShip, new Bitmap("..\\..\\media\\enemyship.png"));
                objectSkins.Add(objectName.EStrongEnemyShip, new Bitmap("..\\..\\media\\strongenemyship.png"));
                objectSkins.Add(objectName.EPowerUp, new Bitmap("..\\..\\media\\powerup.png"));
                objectSkins.Add(objectName.EEnemyBullet, new Bitmap("..\\..\\media\\enemybullet.png"));
                objectSkins.Add(objectName.EPlayerBullet, new Bitmap("..\\..\\media\\playerbullet1.png"));
                objectSkins.Add(objectName.ESceneryBackground, new Bitmap("..\\..\\media\\sceneryBackground.png"));
                objectSkins.Add(objectName.ESceneryObject, new Bitmap("..\\..\\media\\scenery2.png"));
                objectSkins.Add(objectName.EBossShip, new Bitmap("..\\..\\media\\bossship.png"));
                title = new Bitmap("..\\..\\media\\title.png");
            }
            catch
            {
                try
                {
                    objectSkins.Add(objectName.EPlayerShip, new Bitmap(".\\media\\playership.png"));
                    objectSkins.Add(objectName.ENormalEnemyShip, new Bitmap(".\\media\\enemyship.png"));
                    objectSkins.Add(objectName.EStrongEnemyShip, new Bitmap(".\\media\\strongenemyship.png"));
                    objectSkins.Add(objectName.EPowerUp, new Bitmap(".\\media\\powerup.png"));
                    objectSkins.Add(objectName.EEnemyBullet, new Bitmap(".\\media\\enemybullet.png"));
                    objectSkins.Add(objectName.EPlayerBullet, new Bitmap(".\\media\\playerbullet1.png"));
                    objectSkins.Add(objectName.ESceneryBackground, new Bitmap(".\\media\\sceneryBackground.png"));
                    objectSkins.Add(objectName.ESceneryObject, new Bitmap(".\\media\\scenery2.png"));
                    objectSkins.Add(objectName.EBossShip, new Bitmap(".\\media\\bossship.png"));
                    title = new Bitmap(".\\media\\title.png");
                }
                catch
                {
                    Bitmap error = (Bitmap)DrawText("ERROR", new Font("Consolas", 30f), Color.DarkRed, Color.White);
                    objectSkins.Add(objectName.EPlayerShip, error);
                    objectSkins.Add(objectName.ENormalEnemyShip, error);
                    objectSkins.Add(objectName.EStrongEnemyShip, error);
                    objectSkins.Add(objectName.EPowerUp, error);
                    objectSkins.Add(objectName.EEnemyBullet, error);
                    objectSkins.Add(objectName.EPlayerBullet, error);
                    objectSkins.Add(objectName.ESceneryBackground, new Bitmap(screenWidth, screenHeight));
                    objectSkins.Add(objectName.ESceneryObject, error);
                    objectSkins.Add(objectName.EBossShip, error);
                    title = (Bitmap)DrawText("FireShip", new Font("Consolas", 30f), Color.DarkRed, Color.White);
                }
            }

            foreach (KeyValuePair<objectName, Bitmap> entry in objectSkins)
                entry.Value.MakeTransparent();

            BulletMediator.linkLists(playerShipList, enemyShipList);

            for (int i = 0; i < 20; i++)
            {
                SceneryObject obj = SceneryObject.randomSceneryObject();
                obj.setStrategy(MSStraight);
                sceneryObjectList.Add(obj);
            }

            frameCounterLabel.BackColor = Color.Black;
            frameCounterLabel.ForeColor = Color.LightGray;
            frameCounterLabel.Top = 20;
            frameCounterLabel.Left = 20;
            frameCounterLabel.AutoSize = true;
            frameCounterLabel.Font = new Font("Arial", 10);
            scoreLabel.BackColor = Color.Black;
            scoreLabel.ForeColor = Color.LightGray;
            scoreLabel.Top = screenHeight - 74;
            scoreLabel.Left = 20;
            scoreLabel.AutoSize = true;
            scoreLabel.Font = new Font("Arial", 10);
            Controls.Add(frameCounterLabel);
            Controls.Add(scoreLabel);
            frameCounterLabel.BringToFront();
            scoreLabel.BringToFront();
            menu = FireShip.Menu.getInstance();
            titleImage = new PictureBox();
            title.MakeTransparent();
            titleImage.Image = title;
            titleImage.SetBounds(screenWidth / 2 - title.Width / 2, screenHeight / 10, title.Width, title.Height);
            Controls.Add(titleImage);
            titleImage.BringToFront();
            menuIcons = new List<Label>();
            List<string> menuOptions = menu.getOptions();
            for (int i = 0; i < menuOptions.Count + 2; i++)
            {
                Label menuOption = new Label();
                menuOption.BackColor = Color.Black;
                menuOption.ForeColor = Color.LightGray;
                menuOption.TextAlign = ContentAlignment.MiddleCenter;
                menuOption.AutoSize = true;
                menuOption.Font = new Font("Arial", 10);
                menuOption.Refresh();
                menuIcons.Add(menuOption);
                Controls.Add(menuIcons[i]);
                menuIcons[i].BringToFront();
                if (i < menuOptions.Count)
                {
                    menuOption.Text = menuOptions[i];
                    menuOption.Left = screenWidth / 2 - menuOption.Width / 2 + 5;
                    menuOption.Top = screenHeight * 2 / 5 + 30 * i;
                }
                else if (i == menuOptions.Count + 1)
                {
                    menuOption.Text = ">";
                    menuOption.Left = screenWidth / 3;
                    menuOption.Top = screenHeight * 2 / 5;
                }
                else
                {
                    menuOption.Text = "<";
                    menuOption.Left = screenWidth * 2 / 3;
                    menuOption.Top = screenHeight * 2 / 5;
                }
            }

            readHighscores(highscoresNames, highscoresScores);
        }
    }
}
