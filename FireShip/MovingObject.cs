using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace FireShip
{
    public enum EnemyFactoryType
    {
        STRONG,
        NORMAL
    }

    public abstract class EnemyShipFactory
    {
        public static EnemyShipFactory GetEnemyShipFactory(EnemyFactoryType enemyFactoryType, int diffcultyTimeIncrement)
        {
            if (enemyFactoryType == EnemyFactoryType.STRONG)
                return new StrongEnemyShipFactory(diffcultyTimeIncrement);
            else if (enemyFactoryType == EnemyFactoryType.NORMAL)
                return new NormalEnemyShipFactory(diffcultyTimeIncrement);
            else
                return null;
        }

        public abstract EnemyShip CreateEnemy();
    }

    public class StrongEnemyShipFactory : EnemyShipFactory
    {
        private int _difficultyTimeIncrement;

        public StrongEnemyShipFactory(int difficultyTimeIncrement)
        {
            _difficultyTimeIncrement = difficultyTimeIncrement;
        }

        public override EnemyShip CreateEnemy()
        {
            return new StrongEnemyShip(_difficultyTimeIncrement);
        }
    }

    public class NormalEnemyShipFactory : EnemyShipFactory
    {
        private int _difficultyTimeIncrement;

        public NormalEnemyShipFactory(int difficultyTimeIncrement)
        {
            _difficultyTimeIncrement = difficultyTimeIncrement;
        }

        public override EnemyShip CreateEnemy()
        {
            return new NormalEnemyShip(_difficultyTimeIncrement);
        }
    }

    public abstract class MovingObject
    {
        protected Vector position;
        protected Vector previousPosition;
        protected Vector velocity;
        protected double maxVelocity;
        protected double size;
        protected Bitmap image;
        protected ScreenView.objectName name;

        public ScreenView.objectName getName()
        {
            return name;
        }

        public Vector getPosition()
        {
            return position;
        }

        public void setPosition(double x, double y)
        {
            position.X = x;
            position.Y = y;
        }

        public Vector getPreviousPosition()
        {
            return previousPosition;
        }

        public void setPreviousPosition(double x, double y)
        {
            previousPosition.X = x;
            previousPosition.Y = y;
        }

        public Vector getVelocity()
        {
            return velocity;
        }

        public void setVelocity(double x, double y)
        {
            velocity.X = x;
            velocity.Y = y;
        }

        public double getSize()
        {
            return size;
        }

        public void setSize(double newSize)
        {
            size = newSize;
        }

        public double getMaxVelocity()
        {
            return maxVelocity;
        }

        public void setMaxVelocity(double maxVelocity)
        {
            this.maxVelocity = maxVelocity;
        }

        public void setImage(Bitmap image)
        {
            this.image = image;
        }

        public Bitmap getImage()
        {
            return image;
        }

        public void move(double posX, double posY, double velX, double velY)
        {
            setPosition(posX, posY);
            setVelocity(velX, velY);
        }

        public void moveUp(double moveVelocity)
        {
            velocity.Y -= moveVelocity;
            if (velocity.Y > maxVelocity)
                velocity.Y = maxVelocity;
        }

        public void moveDown(double moveVelocity)
        {
            velocity.Y += moveVelocity;
            if (velocity.Y < 0 - maxVelocity)
                velocity.Y = 0 - maxVelocity;
        }

        public void moveLeft(double moveVelocity)
        {
            velocity.X -= moveVelocity;
            if (velocity.X > maxVelocity)
                velocity.X = maxVelocity;
        }

        public void moveRight(double moveVelocity)
        {
            velocity.X += moveVelocity;
            if (velocity.X < 0 - maxVelocity)
                velocity.X = 0 - maxVelocity;
        }
    }

    public class Boss : MovingObject
    {
        private BossState state = null;
        public ScreenView.bossStates stateName;
        private int hp;
        private int def;

        public Boss(int hp, int def, int size)
        {
            name = ScreenView.objectName.EBossShip;
            this.size = 0.0125 * size;
            position.X = 0.55 - this.size/2;
            position.Y = 0 - this.size;
            previousPosition.X = position.X;
            previousPosition.Y = position.Y;
            velocity.X = 0;
            velocity.Y = 0;
            maxVelocity = 0.005;

            this.hp = hp;
            this.def = def;

            state = new BossEntry(this);
            stateName = ScreenView.bossStates.EBossEntry;
            
        }

        public void setHitpoints(int h) { hp = h; }

        public void setDefence(int d) { def = d; }

        public int getHitpoints() { return hp; }

        public int getDefence() { return def; }

        public BossState getState()
        {
            return state;
        }

        public void setState(BossState bs)
        {
            state = bs;
            if (bs.GetType() == typeof(BossBreak))
            {
                stateName = ScreenView.bossStates.EBossBreak;
            }
            if (bs.GetType() == typeof(BossDefense))
            {
                stateName = ScreenView.bossStates.EBossDefense;
            }
            if (bs.GetType() == typeof(BossDeath))
            {
                stateName = ScreenView.bossStates.EBossDeath;
            }
            if (bs.GetType() == typeof(BossEntry))
            {
                stateName = ScreenView.bossStates.EBossEntry;
            }
        }

        public void move()
        {
            if (state != null)
            {
                state.move(this);
            }

        }

        public void shoot()
        {
            if (state != null)
            {
                state.shoot(this);
            }
        }

        public void defend(int damage)
        {
            if (state != null)
            {
                state.defend(this, damage);
            }
        }
    }

    public interface BossState
    {
        void shoot(Boss b);
        void defend(Boss b, int damage);
        void move(Boss b);
    }

    public class BossEntry : BossState
    {
        public BossEntry(Boss b)
        {
            b.setVelocity(0, 0.001);
        }
        
        public void shoot(Boss b)
        {
            //nothing
        }

        public void defend(Boss b, int damage)
        {
            //nothing
        }

        public void move(Boss b)
        {

            double x = b.getPosition().X;
            double y = b.getPosition().Y;

            b.setPreviousPosition(x, y);

            double velX = b.getVelocity().X;
            double velY = b.getVelocity().Y;

            double newX = x + velX;
            double newY = y + velY;

            b.setPosition(newX, newY);

            if (b.getPosition().Y > 0.1)
            {
                b.setState(new BossDefense(b));
            }
        }
    }

    public class BossDeath : BossState
    {

        public BossDeath(Boss b)
        {
            b.setVelocity(0, -0.001);
        }

        public void shoot(Boss b)
        {
            //nothing
        }

        public void defend(Boss b, int damage)
        {
            //nothing
        }

        public void move(Boss b)
        {
            if (b.getPosition().Y < -0.2)
            {
                ScreenView.boss = null;
            }

            double x = b.getPosition().X;
            double y = b.getPosition().Y;

            b.setPreviousPosition(x, y);

            double velX = b.getVelocity().X;
            double velY = b.getVelocity().Y;

            double newX = x + velX;
            double newY = y + velY;

            b.setPosition(newX, newY);
        }
    }

    public class BossDefense : BossState
    {
        public BossDefense(Boss b)
        {
            b.setVelocity(0.0005, 0);
        }

        public void shoot(Boss b)
        {
            if(ScreenView.rnd.Next(3) == 1)
                BulletMediator.bossShot(b);
        }

        public void defend(Boss b, int damage)
        {
            if (damage >= b.getDefence())
            {
                b.setState(new BossBreak(b));
                b.setDefence(0);
                
            }
            else
            {
                b.setDefence(b.getDefence() - damage);
            }
        }

        public void move(Boss b)
        {
            if (b.getPosition().X > 0.8)
            {
                b.setVelocity(-0.0005, 0);
            }
            else if (b.getPosition().X < 0.2)
            {
                b.setVelocity(0.0005, 0);
            }
            double x = b.getPosition().X;
            double y = b.getPosition().Y;

            b.setPreviousPosition(x, y);

            double velX = b.getVelocity().X;
            double velY = b.getVelocity().Y;

            double newX = x + velX;
            double newY = y + velY;

            b.setPosition(newX, newY);
        }
    }

    public class BossBreak : BossState
    {
        public BossBreak(Boss b)
        {
            b.setVelocity(0.0025, 0);
        }

        public void shoot(Boss b)
        {
            BulletMediator.bossShot(b);
        }

        public void defend(Boss b, int damage)
        {
            if (damage >= b.getHitpoints())
            {
                b.setState(new BossDeath(b));
                b.setHitpoints(0);
                
            }
            else
            {
                b.setHitpoints(b.getHitpoints() - damage);

            }
        }

        public void move(Boss b)
        {
            if (b.getPosition().X > 1)
            {
                b.setVelocity(-0.0025, b.getVelocity().Y);
            }
            else if (b.getPosition().X < 0)
            {
                b.setVelocity(0.0025, b.getVelocity().Y);
            }
            if (b.getPosition().Y > 0.5)
            {
                b.setVelocity(b.getVelocity().X, -b.getVelocity().Y);
            }
            else if (b.getPosition().Y < 0.1)
            {
                b.setVelocity(b.getVelocity().X, -b.getVelocity().Y);
            }

            double x = b.getPosition().X;
            double y = b.getPosition().Y;

            b.setPreviousPosition(x, y);

            double velX = b.getVelocity().X;
            double velY = b.getVelocity().Y;

            double randX = (ScreenView.rnd.Next(10) - ScreenView.rnd.Next(10)) / 50000.0;
            double randY = (ScreenView.rnd.Next(10) - ScreenView.rnd.Next(10)) / 50000.0;

            if (velX + randX > 0.005 || velX + randX < 0.002)
                velX -= randX;
            if (velY + randY > 0.005 || velY + randY < 0.002)
                velY -= randY;

            b.setVelocity(velX, velY);

            double newX = x + velX;
            double newY = y + velY;

            b.setPosition(newX, newY);
        }
    }

    public abstract class MovementStrategy
    {
        public abstract void move(AutomatedMovingObject obj);
    }

    public class MovementStrategyRandom : MovementStrategy
    {
        public override void move(AutomatedMovingObject obj)
        {
            double x = obj.getPosition().X;
            double y = obj.getPosition().Y;

            obj.setPreviousPosition(x, y);

            double velX = obj.getVelocity().X;
            double velY = obj.getVelocity().Y;


            double angle = (ScreenView.rnd.NextDouble() - ScreenView.rnd.NextDouble()) * Math.PI * 2 * 0.05;
            double newVX = velX * Math.Cos(angle) - velY * Math.Sin(angle);
            double newVY = velX * Math.Sin(angle) + velY * Math.Cos(angle);

            x += newVX;
            y += newVY;

            if (x < 0) { x = -x; newVX = -newVX; }
            if (x > 1) { x = 2 - x; newVX = -newVX; }
            if (y < 0 - obj.getSize()) { y = 0 - obj.getSize(); newVY = -newVY; }

            obj.setPosition(x, y);
            obj.setVelocity(newVX, newVY);
        }
    }

    public class MovementStrategyWave : MovementStrategy
    {
        public override void move(AutomatedMovingObject obj)
        {
            double x = obj.getPosition().X;
            double y = obj.getPosition().Y;

            obj.setPreviousPosition(x, y);

            double velX = obj.getVelocity().X;
            double velY = obj.getVelocity().Y;

            if (velY <= 0.0005 && velX < 0)
            {
                velY = 0.0005;
                obj.movementPhase = 1;
            }
            else if (velY <= 0.0005 && velX > 0)
            {
                velY = 0.0005;
                obj.movementPhase = 0;
            }


            double angle = 0;

            if (obj.movementPhase == 0)
                angle = 0.05 * Math.PI * 2 * 0.05;
            else if (obj.movementPhase == 1)
                angle = -0.05 * Math.PI * 2 * 0.05;

            double newVX = velX * Math.Cos(angle) - velY * Math.Sin(angle);
            double newVY = velX * Math.Sin(angle) + velY * Math.Cos(angle);

            x += newVX;
            y += newVY;

            if (x < 0) { x = -x; newVX = -newVX; }
            if (x > 1) { x = 2 - x; newVX = -newVX; }
            if (y < 0 - obj.getSize()) { y = 0 - obj.getSize(); newVY = -newVY; }

            obj.setPosition(x, y);
            obj.setVelocity(newVX, newVY);
        }
    }

    public class MovementStrategyStraight : MovementStrategy
    {
        public override void move(AutomatedMovingObject obj)
        {
            double x = obj.getPosition().X;
            double y = obj.getPosition().Y;

            obj.setPreviousPosition(x, y);

            double velX = obj.getVelocity().X;
            double velY = obj.getVelocity().Y;

            double newX = x + velX;
            double newY = y + velY;

            obj.setPosition(newX, newY);
        }
    }

    public abstract class AutomatedMovingObject : MovingObject
    {
        protected MovementStrategy MS = null;
        public int movementPhase = 0;

        public void setStrategy(MovementStrategy movementStrategy)
        {
            MS = movementStrategy;
        }
        public MovementStrategy getStrategy()
        {
            return MS;
        }

        public void move()
        {
            if (this != null && MS != null)
                MS.move(this);
        }
    }

    public abstract class EnemyShip : AutomatedMovingObject
    {
        protected int hitPoints;

        protected EnemyShip()
        {
            
        }

        public void shoot()
        {
            BulletMediator.enemyShot(this);
        }

        public int getHitpoints() { return hitPoints; }

        public int takeDamage(int damage)
        {
            hitPoints -= damage;
            return hitPoints;
        }
    }

    public class StrongEnemyShip : EnemyShip
    {
        public StrongEnemyShip(int difficultyTimeIncrement)
        {
            name = ScreenView.objectName.EStrongEnemyShip;
            hitPoints = 1 + (int)ScreenView.gameTime.ElapsedMilliseconds / (difficultyTimeIncrement * 2000);
            hitPoints = 2 + 2 * hitPoints;
            size = 0.0125 * 6;

            double tempPosX = ScreenView.rnd.NextDouble();
            if (tempPosX < 0.1)
                tempPosX += 0.1;
            else
            if (tempPosX > 0.9)
                tempPosX -= 0.1;

            position.X = tempPosX;
            position.Y = 0 - size;
            previousPosition.X = position.X;
            previousPosition.Y = position.Y;
            velocity.X = 0;
            velocity.Y = 0.001 + 0.0006 * (int)ScreenView.gameTime.ElapsedMilliseconds / (difficultyTimeIncrement * 3 * 1000);
            maxVelocity = 0.001;
        }
    }

    public class NormalEnemyShip : EnemyShip
    {
        public NormalEnemyShip(int difficultyTimeIncrement)
        {
            name = ScreenView.objectName.ENormalEnemyShip;
            hitPoints = 1 + (int)ScreenView.gameTime.ElapsedMilliseconds / (difficultyTimeIncrement * 2000);
            size = 0.0125 * 3;

            double tempPosX = ScreenView.rnd.NextDouble();
            if (tempPosX < 0.1)
                tempPosX += 0.1;
            else
            if (tempPosX > 0.9)
                tempPosX -= 0.1;

            position.X = tempPosX;
            position.Y = 0 - size;
            previousPosition.X = position.X;
            previousPosition.Y = position.Y;
            velocity.X = 0;
            velocity.Y = 0.001 + 0.0006 * (int)ScreenView.gameTime.ElapsedMilliseconds / (difficultyTimeIncrement * 1000);
            maxVelocity = 0.01;
        }
    }

    public class PlayerShip : MovingObject, IPlayerShip
    {
        private static PlayerShip instance = null;

        int power;
        double fireRate;
        double speed;
        int multipleBullets;
        double moveVelocity;
        Stopwatch shotCooldown;

        private PlayerShip(int playerSize)
        {
            name = ScreenView.objectName.EPlayerShip;
            position.X = 0.5 - 0.0125;
            position.Y = 0.8;
            previousPosition.X = position.X;
            previousPosition.Y = position.Y;
            velocity.X = 0;
            velocity.Y = 0;
            maxVelocity = 0.005;
            moveVelocity = 0.002;
            size = 0.0125 * playerSize;

            power = 1;
            fireRate = 1; // 1
            shotCooldown = new Stopwatch();
            shotCooldown.Start();
            speed = 1;
            multipleBullets = 1; // 1
        }

        public static PlayerShip getInstance()
        {
            if (instance == null)
                instance = new PlayerShip(5);

            return instance;
        }

        public static PlayerShip getReset()
        {
            instance = new PlayerShip(5);
            return instance;
        }

        public int getPower() { return power; }
        public double getFirerate() { return fireRate; }
        public double getSpeed() { return speed; }
        public int getMultipleBullets() { return multipleBullets; }
        public Stopwatch getShotCooldown() { return shotCooldown; }
        public double getMoveVelocity() { return moveVelocity; }


        public void moveUp()
        {
            velocity.Y -= getMoveVelocity() * getSpeed() / 2;
            if (getVelocity().Y < 0 - maxVelocity * getSpeed())
                velocity.Y = 0 - maxVelocity * getSpeed();
        }

        public void moveDown()
        {
            velocity.Y += getMoveVelocity() * getSpeed() / 2;
            if (getVelocity().Y > maxVelocity * getSpeed())
                velocity.Y = maxVelocity * getSpeed();
        }

        public void moveLeft()
        {
            velocity.X -= getMoveVelocity() * getSpeed() / 2;
            if (getVelocity().X < 0 - maxVelocity * getSpeed())
                velocity.X = 0 - maxVelocity * getSpeed();
        }

        public void moveRight()
        {
            velocity.X += getMoveVelocity() * getSpeed() / 2;
            if (getVelocity().X > maxVelocity * getSpeed())
                velocity.X = maxVelocity * getSpeed();
        }

        public void shoot()
        {
            if (shotCooldown.ElapsedMilliseconds >= 1000 / fireRate)
            {
                shotCooldown.Restart();
                BulletMediator.playerShot(this);
            }
        }
    }

    public interface IPlayerShip
    {
        int getPower();
        double getFirerate();
        double getSpeed();
        int getMultipleBullets();

        double getSize();

        Vector getPosition();
        void setPosition(double x, double y);

        Vector getVelocity();
        void setVelocity(double x, double y);

        double getMoveVelocity();

        Stopwatch getShotCooldown();

        void setPreviousPosition(double x, double y);

        void moveLeft();
        void moveRight();
        void moveUp();
        void moveDown();

        void shoot();
    }

    public abstract class PlayerPowerUpDecorator : MovingObject, IPlayerShip
    {
        protected IPlayerShip player;

        public PlayerPowerUpDecorator()
        {
            
        }

        public PlayerPowerUpDecorator(IPlayerShip player)
        {
            this.player = player;
        }

        public abstract double getFirerate();
        public abstract int getMultipleBullets();
        public abstract int getPower();
        public abstract double getSpeed();

        public void moveDown()
        {
            setVelocity(getVelocity().X, getVelocity().Y + getMoveVelocity() * getSpeed() / 2);
            if (getVelocity().Y > 0.005 * getSpeed())
                setVelocity(getVelocity().X, 0.005 * getSpeed());
        }

        public void moveLeft()
        {
            setVelocity(getVelocity().X - getMoveVelocity() * getSpeed() / 2, getVelocity().Y);
            if (getVelocity().X < 0 - 0.005 * getSpeed())
                setVelocity(0 - 0.005 * getSpeed(), getVelocity().Y);
        }

        public void moveRight()
        {
            setVelocity(getVelocity().X + getMoveVelocity() * getSpeed() / 2, getVelocity().Y);
            if (getVelocity().X > 0.005 * getSpeed())
                setVelocity(0.005 * getSpeed(), getVelocity().Y);
        }

        public void moveUp()
        {
            setVelocity(getVelocity().X, getVelocity().Y - getMoveVelocity() * getSpeed() / 2);
            if (getVelocity().Y < 0 - 0.005 * getSpeed())
                setVelocity(getVelocity().X, 0 - 0.005 * getSpeed());
        }

        public void shoot()
        {
            if (getShotCooldown().ElapsedMilliseconds >= 1000 / getFirerate())
            {
                getShotCooldown().Restart();
                BulletMediator.playerShot(this);
            }
        }

        public new Vector getPosition()
        {
            return player.getPosition();
        }

        public new double getSize()
        {
            return player.getSize();
        }

        public new void setPosition(double x, double y)
        {
            player.setPosition(x, y);
        }

        public new Vector getVelocity()
        {
            return player.getVelocity();
        }

        public new void setVelocity(double x, double y)
        {
            player.setVelocity(x, y);
        }

        public new void setPreviousPosition(double x, double y)
        {
            player.setPreviousPosition(x, y);
        }

        public Stopwatch getShotCooldown()
        {
            return player.getShotCooldown();
        }

        public double getMoveVelocity()
        {
            return player.getMoveVelocity();
        }
    }

    public class PlayerShipPowerPowerUp : PlayerPowerUpDecorator
    {
        public PlayerShipPowerPowerUp(IPlayerShip player) : base(player)
        {

        }

        public override double getFirerate()
        {
            return player.getFirerate();
        }

        public override int getMultipleBullets()
        {
            return player.getMultipleBullets();
        }

        public override int getPower()
        {
            return player.getPower() + 1;
        }

        public override double getSpeed()
        {
            return player.getSpeed();
        }
    }

    public class PlayerShipFireRatePowerUp : PlayerPowerUpDecorator
    {
        public PlayerShipFireRatePowerUp(IPlayerShip player) : base(player)
        {

        }

        public override double getFirerate()
        {
            return player.getFirerate() + 0.3;
        }

        public override int getMultipleBullets()
        {
            return player.getMultipleBullets();
        }

        public override int getPower()
        {
            return player.getPower();
        }

        public override double getSpeed()
        {
            return player.getSpeed();
        }
    }

    public class PlayerShipSpeedPowerUp : PlayerPowerUpDecorator
    {
        public PlayerShipSpeedPowerUp(IPlayerShip player) : base(player)
        {

        }

        public override double getFirerate()
        {
            return player.getFirerate();
        }

        public override int getMultipleBullets()
        {
            return player.getMultipleBullets();
        }

        public override int getPower()
        {
            return player.getPower();
        }

        public override double getSpeed()
        {
            return player.getSpeed() + 0.2;
        }
    }

    public class PlayerShipMultipleBulletsPowerUp : PlayerPowerUpDecorator
    {
        public PlayerShipMultipleBulletsPowerUp(IPlayerShip player) : base(player)
        {

        }

        public override double getFirerate()
        {
            return player.getFirerate();
        }

        public override int getMultipleBullets()
        {
            return player.getMultipleBullets() + 1;
        }

        public override int getPower()
        {
            return player.getPower();
        }

        public override double getSpeed()
        {
            return player.getSpeed();
        }
    }

    public class BulletMediator
    {
        private static List<MovingObject> playerBulletList;
        private static List<MovingObject> enemyBulletList;
        public static void linkLists(List<MovingObject> newPlayerBulletList, List<MovingObject> newEnemyBulletList)
        {
            playerBulletList = newPlayerBulletList;
            enemyBulletList = newEnemyBulletList;
        }
        public static void playerShot(IPlayerShip ship)
        {
            Vector bulletPosition = new Vector(ship.getPosition(), 0, 0 - (ship.getSize() + 0.0125) / 2);
            int bulletCount = ship.getMultipleBullets();
            Console.Out.WriteLine("Bullet count = " + bulletCount);
            Console.Out.WriteLine("Firerate = " + ship.getFirerate());
            Console.Out.WriteLine("Power = " + ship.getPower());
            Console.Out.WriteLine("Velocity = " + ship.getVelocity().X + ", " + ship.getVelocity().Y);

            if (bulletCount >= 1)
            {
                PlayerBullet b = new PlayerBullet(bulletPosition, new Vector(0, -0.01), ship.getPower(), '1');
                b.setStrategy(ScreenView.MSStraight);
                playerBulletList.Add(b);
            }
            if (bulletCount >= 2)
            {
                PlayerBullet b = new PlayerBullet(bulletPosition, new Vector(-0.007, -0.007), ship.getPower(), '2');
                b.setStrategy(ScreenView.MSStraight);
                playerBulletList.Add(b);
            }
            if (bulletCount >= 3)
            {
                PlayerBullet b = new PlayerBullet(bulletPosition, new Vector(0.007, -0.007), ship.getPower(), '3');
                b.setStrategy(ScreenView.MSStraight);
                playerBulletList.Add(b);
            }
            if (bulletCount >= 4)
            {
                PlayerBullet b = new PlayerBullet(bulletPosition, new Vector(0.01, 0), ship.getPower(), '4');
                b.setStrategy(ScreenView.MSStraight);
                playerBulletList.Add(b);
            }
            if (bulletCount >= 5)
            {
                PlayerBullet b = new PlayerBullet(bulletPosition, new Vector(-0.01, 0), ship.getPower(), '5');
                b.setStrategy(ScreenView.MSStraight);
                playerBulletList.Add(b);
            }
        }
        public static void enemyShot(EnemyShip ship)
        {
            Vector bulletPosition = new Vector(ship.getPosition(), 0, (ship.getSize() + 0.0125) / 2);
            EnemyBullet bullet = new EnemyBullet(bulletPosition, new Vector(0, 0.01), 1);
            bullet.setStrategy(ScreenView.MSStraight);
            enemyBulletList.Add(bullet);
        }

        public static void bossShot(Boss ship)
        {
            Vector bulletPosition = new Vector(ship.getPosition(), 0, (ship.getSize() + 0.0125) / 2);
            EnemyBullet bullet = new EnemyBullet(bulletPosition, new Vector(0, 0.01), 1);
            bullet.setStrategy(ScreenView.MSStraight);
            enemyBulletList.Add(bullet);
        }
    }

    public abstract class Bullet : AutomatedMovingObject
    {
        protected int damage;

        protected Bullet(Vector position, Vector velocity, int damage)
        {
            this.position.X = position.X;
            this.position.Y = position.Y;
            previousPosition.X = -1;
            previousPosition.Y = -1;
            this.velocity.X = velocity.X;
            this.velocity.Y = velocity.Y;
            maxVelocity = 0.04;
            size = 0.0125;
            this.damage = damage;
        }

        public int getDamage() { return damage; }
        public abstract char getShape();
    }

    public class EnemyBullet : Bullet
    {
        public EnemyBullet(Vector position, Vector velocity, int damage) : base(position, velocity, damage)
        {
            name = ScreenView.objectName.EEnemyBullet;
        }

        public override char getShape()
        {
            return '1';
        }
    }

    public class PlayerBullet : Bullet
    {
        protected char type;

        public PlayerBullet(Vector position, Vector velocity, int damage, char type) : base(position, velocity, damage)
        {
            name = ScreenView.objectName.EPlayerBullet;
            this.type = type;
        }

        public override char getShape()
        {
            return type;
        }
    }

    public class SceneryObject : AutomatedMovingObject
    {
        static List<Bitmap> imgList = new List<Bitmap>();
        public int depth;

        public SceneryObject()
        {
            name = ScreenView.objectName.ESceneryObject;
            size = 0.0125;
            depth = 0;

            double randomShape = ScreenView.rnd.NextDouble();

            try { image = imgList[1]; }
            catch { };
            size = randomShape / 100;
            if (size < 0.0045)
            {
                size = 0.0045;
                velocity.Y = 0.0005;
            }
            else
                velocity.Y = randomShape / 1000;

            double tempPosX = ScreenView.rnd.NextDouble();
            if (tempPosX < 0.02)
                tempPosX += 0.02;
            else
            if (tempPosX > 0.98)
                tempPosX -= 0.02;

            position.X = tempPosX;
            position.Y = 0 - size;
            previousPosition.X = position.X;
            previousPosition.Y = position.Y;
            velocity.X = 0;
            maxVelocity = 0.1;
        }

        private SceneryObject(double top, int depth)
        {
            name = ScreenView.objectName.ESceneryObject;
            size = 0.0125;
            this.depth = depth;

            double randomShape = ScreenView.rnd.NextDouble();

            try { image = imgList[1]; }
            catch { };
            size = randomShape / 100;
            if (size < 0.0045)
            {
                size = 0.0045;
                velocity.Y = 0.0005;
            }
            else
                velocity.Y = randomShape / 1000;

            double tempPosX = ScreenView.rnd.NextDouble();
            if (tempPosX < 0.02)
                tempPosX += 0.02;
            else
            if (tempPosX > 0.98)
                tempPosX -= 0.02;

            position.X = tempPosX;
            position.Y = top;
            previousPosition.X = position.X;
            previousPosition.Y = position.Y;
            velocity.X = 0;
            maxVelocity = 0.1;
        }

        public SceneryObject(Vector velocity, Vector position, int depth)
        {
            name = ScreenView.objectName.ESceneryObject;
            size = 0.0125;
            this.depth = depth;
            double tempPosX = ScreenView.rnd.NextDouble();
            if (tempPosX < 0.02)
                tempPosX += 0.02;
            else
            if (tempPosX > 0.98)
                tempPosX -= 0.02;

            this.position.X = position.X;
            this.position.Y = position.Y;
            previousPosition.X = position.X;
            previousPosition.Y = position.Y;
            this.velocity.X = velocity.X;
            this.velocity.Y = velocity.Y;
            maxVelocity = 0.1;
        }

        public static SceneryObject randomSceneryObject()
        {
            double tempPosY = ScreenView.rnd.NextDouble();
            SceneryObject randomObject = new SceneryObject(tempPosY, 0);
            return randomObject;
        }

        public static void setImageList(List<Bitmap> list) { imgList = list; }
        public int getDepth() { return depth; }
    }

    public class PowerUp : AutomatedMovingObject
    {
        protected char shape;

        public PowerUp(IPlayerShip ship)
        {
            name = ScreenView.objectName.EPowerUp;
            size = 0.0125 * 3;

            double tempPosX = ScreenView.rnd.NextDouble();
            if (tempPosX < 0.1)
                tempPosX += 0.1;
            else
            if (tempPosX > 0.9)
                tempPosX -= 0.1;

            position.X = tempPosX;
            position.Y = 0 - size;
            previousPosition.X = position.X;
            previousPosition.Y = position.Y;
            velocity.X = 0;
            velocity.Y = 0.002;
            maxVelocity = 0.01;

            double value = ScreenView.rnd.NextDouble();

            if (value < 0.2)
            {
                shape = 'S';
            }
            else if (value < 0.5)
            {
                if (ship.getMultipleBullets() < 5)
                {
                    shape = 'M';
                }
                else
                {
                    shape = 'P';
                }
            }
            else if (value < 0.8)
            {
                shape = 'F';
            }
            else if (value <= 1)
            {
                shape = 'P';
            }
        }

        public PowerUp(char shape)
        {
            name = ScreenView.objectName.EPowerUp;
            size = 0.0125 * 3;

            double tempPosX = ScreenView.rnd.NextDouble();
            if (tempPosX < 0.1)
                tempPosX += 0.1;
            else
            if (tempPosX > 0.9)
                tempPosX -= 0.1;

            position.X = tempPosX;
            position.Y = 0 - size;
            previousPosition.X = position.X;
            previousPosition.Y = position.Y;
            velocity.X = 0;
            velocity.Y = 0.002;
            maxVelocity = 0.01;
            this.shape = shape;
        }

        public char getShape() { return shape; }
    }

    public class Menu
    {
        private static Menu instance;
        protected List<string> menuList = new List<string>();
        protected int selection;
        protected int previousSelection;
        protected bool selectionChanged;
        protected bool highscoresOn;

        private Menu()
        {
            menuList.Add("S t a r t");
            menuList.Add("H i g h s c o r e s");
            menuList.Add("E x i t");

            selection = 0;
            selectionChanged = false;
            highscoresOn = false;
        }

        public static Menu getInstance()
        {
            if (instance == null)
                instance = new Menu();

            return instance;
        }

        public List<string> getOptions() { return menuList; }
        public void setHighscoresOn() { highscoresOn = true; }
        public void setHighscoresOff() { highscoresOn = false; }

        public int selectUp()
        {
            previousSelection = selection;

            selection--;
            if (selection < 0)
                selection = menuList.Count - 1;

            selectionChanged = true;
            return selection;
        }

        public int selectDown()
        {
            previousSelection = selection;

            selection++;
            selection = selection % menuList.Count;

            selectionChanged = true;
            return selection;
        }

        public int getSelection()
        {
            if (highscoresOn)
            {
                selection = 1;
                return 3;
            }

            return selection;
        }
    }
}
