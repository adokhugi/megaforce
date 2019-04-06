using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame2
{
    class Character
    {
        public const int OFFSETX = 0;
        public const int OFFSETY = -12;
        public const int MAXNUMBER_ITEMS = 4;
        public const int MAXNUMBER_WEAPONS = 4;
        public const int MAXNUMBER_RINGS = 4;
        public const int MAXNUMBER_SPELLS = 4;
        public const int LEVELATWHICHPROMOTED_DEFAULT = 20;
        public const int LEVELATWHICHPROMOTED_ELRIC = 21;
        public const int LEVELATWHICHPROMOTED_KARNA = 24;
        public const int LEVELATWHICHPROMOTED_JANET = 24;
        public const int LEVELATWHICHPROMOTED_ERIC = 24;
        public const int LEVELATWHICHPROMOTED_RANDOLF = 24;
        public const int LEVELATWHICHPROMOTED_TYRIN = 24;

        public enum PositionStatuses
        {
            LooksDown1,
            LooksDown2,
            LooksUp1,
            LooksUp2,
            LooksLeft1,
            LooksLeft2,
            LooksRight1,
            LooksRight2
        }

        public enum PromotedStatuses
        {
            No,
            YesRegular,
            YesAlternative
        }

        public enum Enemies
        {
            Ninja,
            Banana,
            Witch,
            Samurai,
            Stanes,
            Russell
        }

        public static Texture2D[] TexturesJacob = new Texture2D[8];
        public static Texture2D TexturesJacob_Portrait;
        public static Texture2D[] TexturesCaryn = new Texture2D[8];
        public static Texture2D TexturesCaryn_Portrait;
        public static Texture2D[] TexturesTassi = new Texture2D[8];
        public static Texture2D TexturesTassi_Portrait;
        public static Texture2D[] TexturesEva = new Texture2D[8];
        public static Texture2D TexturesEva_Portrait;
        public static Texture2D[] TexturesHans = new Texture2D[8];
        public static Texture2D TexturesHans_Portrait;
        public static Texture2D[] TexturesMonica = new Texture2D[8];
        public static Texture2D TexturesMonica_Portrait;

        public static Texture2D[] TexturesNinja = new Texture2D[8];
        public static Texture2D[] TexturesBanana = new Texture2D[8];
        public static Texture2D[] TexturesWitch = new Texture2D[8];
        public static Texture2D[] TexturesSamurai = new Texture2D[8];
        public static Texture2D[] TexturesStanes = new Texture2D[8];
        public static Texture2D[] TexturesRussell = new Texture2D[8];

        private String _name;

        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private String _charClass;

        public String CharClass
        {
            get { return _charClass; }
            set { _charClass = value; }
        }

        private int _level;

        public int Level
        {
            get { return _level; }
            set { while (_level < value) NextLevel(); }
        }

        private int _hitPoints;

        public int HitPoints
        {
            get { return _hitPoints; }
            set { _hitPoints = value; }
        }

        private int _maxHitPoints;

        public int MaxHitPoints
        {
            get { return _maxHitPoints; }
            set { _maxHitPoints = value; }
        }

        private int _attackPoints;

        public int AttackPoints
        {
            get { return _attackPoints; }
            set { _attackPoints = value; }
        }

        private int _defensePoints;

        public int DefensePoints
        {
            get { return _defensePoints; }
            set { _defensePoints = value; }
        }

        private int _magicPoints;

        public int MagicPoints
        {
            get { return _magicPoints; }
            set { _magicPoints = value; }
        }

        private int _maxMagicPoints;

        public int MaxMagicPoints
        {
            get { return _maxMagicPoints; }
            set { _maxMagicPoints = value; }
        }

        private bool _mustSurvive;

        public bool MustSurvive
        {
            get { return _mustSurvive; }
            set { _mustSurvive = value; }
        }

        private int _movePoints;

        public int MovePoints
        {
            get { return _movePoints; }
            set { _movePoints = value; }
        }

        private int _agility;

        public int Agility
        {
            get { return _agility; }
            set { _agility = value; }
        }

        private bool _flying;

        public bool Flying
        {
            get { return _flying; }
            set { _flying = value; }
        }

        // order of textures: down down up up left left right right
        private Texture2D[] _texture = new Texture2D[8];

        private float _position;

        public float Position
        {
            get { return _position; }
            set { _position = value; }
        }

        private PositionStatuses _positionStatus;

        public PositionStatuses PositionStatus
        {
            get { return _positionStatus; }
            set { _positionStatus = value; }
        }

        private bool _alive;

        public bool Alive
        {
            get { return _alive; }
            set { _alive = value; }
        }

        private bool _visible;

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        private int _numKills;

        public int NumKills
        {
            get { return _numKills; }
            set { _numKills = value; }
        }

        private int _numDefeats;

        public int NumDefeats
        {
            get { return _numDefeats; }
            set { _numDefeats = value; }
        }

        private int _experiencePoints;

        public int ExperiencePoints
        {
            get { return _experiencePoints; }
            set { _experiencePoints = value; }
        }

        private int _minAttackRange;

        public int MinAttackRange
        {
            get { return _minAttackRange; }
            set { _minAttackRange = value; }
        }

        private int _maxAttackRange;

        public int MaxAttackRange
        {
            get { return _maxAttackRange; }
            set { _maxAttackRange = value; }
        }

        private int _gold;

        public int Gold
        {
            get { return _gold; }
            set { _gold = value; }
        }

        public Item[] Items = new Item[MAXNUMBER_ITEMS];

        public bool[] ItemEquipped = new bool[MAXNUMBER_ITEMS];

        public Spell[] MagicSpells = new Spell[MAXNUMBER_SPELLS];

        private Texture2D _portrait;

        public Texture2D Portrait
        {
            get { return _portrait; }
            set { _portrait = value; }
        }

        private int _asleep;

        public int Asleep
        {
            get { return _asleep; }
            set { _asleep = value; }
        }

        private bool _poisoned;

        public bool Poisoned
        {
            get { return _poisoned; }
            set { _poisoned = value; }
        }

        private bool _cursed;

        public bool Cursed
        {
            get { return _cursed; }
            set { _cursed = value; }
        }

        private int _stunned;

        public int Stunned
        {
            get { return _stunned; }
            set { _stunned = value; }
        }

        private int _silenced;

        public int Silenced
        {
            get { return _silenced; }
            set { _silenced = value; }
        }

        private int _confused;

        public int Confused
        {
            get { return _confused; }
            set { _confused = value; }
        }

        private int _oldMaxHitPoints;

	    public int OldMaxHitPoints
	    {
		    get { return _oldMaxHitPoints;}
		    set { _oldMaxHitPoints = value;}
	    }

        private int _oldMaxMagicPoints;

    	public int OldMaxMagicPoints
	    {
		    get { return _oldMaxMagicPoints;}
		    set { _oldMaxMagicPoints = value;}
	    }

        private int _oldAgility;

    	public int OldAgility
	    {
		    get { return _oldAgility;}
		    set { _oldAgility = value;}
	    }
        
        private int _oldAttackPoints;

    	public int OldAttackPoints
	    {
		    get { return _oldAttackPoints;}
		    set { _oldAttackPoints = value;}
	    }

        private int _oldDefensePoints;

        public int OldDefensePoints
	    {
            get { return _oldDefensePoints; }
            set { _oldDefensePoints = value; }
	    }

        private int _levelToDisplay;

        public int LevelToDisplay
        {
            get { return _levelToDisplay; }
            set { _levelToDisplay = value; }
        }

        private PromotedStatuses _promotedStatus;

        public PromotedStatuses PromotedStatus
        {
            get { return _promotedStatus; }
            set { _promotedStatus = value; }
        }

        private bool _susceptibleFire;

        public bool SusceptibleFire
        {
            get { return _susceptibleFire; }
            set { _susceptibleFire = value; }
        }

        private bool _susceptibleIce;

        public bool SusceptibleIce
        {
            get { return _susceptibleIce; }
            set { _susceptibleIce = value; }
        }

        private bool _canPoison;

        public bool CanPoison
        {
            get { return _canPoison; }
            set { _canPoison = value; }
        }

        private bool _canSlow;

        public bool CanSlow
        {
            get { return _canSlow; }
            set { _canSlow = value; }
        }

        private bool _canStun;

        public bool CanStun
        {
            get { return _canStun; }
            set { _canStun = value; }
        }

        private bool _canMuddle;

        public bool CanMuddle
        {
            get { return _canMuddle; }
            set { _canMuddle = value; }
        }

        private bool _canDispel;

        public bool CanDispel
        {
            get { return _canDispel; }
            set { _canDispel = value; }
        }

        private bool _canSleep;

        public bool CanSleep
        {
            get { return _canSleep; }
            set { _canSleep = value; }
        }

        private bool _resistantFire;

        public bool ResistantFire
        {
            get { return _resistantFire; }
            set { _resistantFire = value; }
        }

        private bool _resistantIce;

        public bool ResistantIce
        {
            get { return _resistantIce; }
            set { _resistantIce = value; }
        }

        private bool _boss;

        public bool Boss
        {
            get { return _boss; }
            set { _boss = value; }
        }

        private int _boosted;

        public int Boosted
        {
            get { return _boosted; }
            set { _boosted = value; }
        }

        private int _attackBoosted;

        public int AttackBoosted
        {
            get { return _attackBoosted; }
            set { _attackBoosted = value; }
        }

        private int _agilityBoostedBy;

        public int AgilityBoostedBy
        {
            get { return _agilityBoostedBy; }
            set { _agilityBoostedBy = value; }
        }

        private int _defenseBoostedBy;

        public int DefenseBoostedBy
        {
            get { return _defenseBoostedBy; }
            set { _defenseBoostedBy = value; }
        }

        private int _attackBoostedBy;

        public int AttackBoostedBy
        {
            get { return _attackBoostedBy; }
            set { _attackBoostedBy = value; }
        }

        private bool _hasSpecialAttack;

        public bool HasSpecialAttack
        {
            get { return _hasSpecialAttack; }
            set { _hasSpecialAttack = value; }
        }

        // constructor for player party members
        public Character(String name, int level, int levelToDisplay, PromotedStatuses promotedStatus, int numKills, int numDefeats, int experiencePoints, Item[] items, bool[] itemEquipped)
        {
            switch (name)
            {
                case "JACOB":
                    Init("JACOB", "SDMN", 1, 1, 12, 12, 6, 4, 8, 8, true, 6, 4, false, true, numKills, numDefeats, experiencePoints, false, items, itemEquipped, new Spell[] { new Spell("BLAZE", 1), null, null, null }, TexturesJacob, TexturesJacob_Portrait);
                    break;

                case "CARYN":
                    Init("CARYN", "PRST", 1, 1, 11, 11, 6, 5, 10, 10, false, 5, 5, false, true, numKills, numDefeats, experiencePoints, false, items, itemEquipped, new Spell[] { new Spell("HEAL", 1), null, null, null }, TexturesCaryn, TexturesCaryn_Portrait);
                    break;

                case "TASSI":
                    Init("TASSI", "WARR", 1, 1, 9, 9, 9, 7, 0, 0, false, 5, 4, false, true, numKills, numDefeats, experiencePoints, false, items, itemEquipped, null, TexturesTassi, TexturesTassi_Portrait);
                    break;

                case "EVA":
                    Init("EVA", "ACHR", 1, 1, 8, 8, 7, 6, 0, 0, false, 5, 4, false, true, numKills, numDefeats, experiencePoints, false, items, itemEquipped, null, TexturesEva, TexturesEva_Portrait);
                    break;

                case "HANS":
                    Init("HANS", "WARR", 1, 1, 8, 8, 12, 6, 0, 0, false, 5, 4, false, true, numKills, numDefeats, experiencePoints, false, items, itemEquipped, null, TexturesHans, TexturesHans_Portrait);
                    break;

                case "MONICA":
                    Init("MONICA", "MAGE", 1, 1, 8, 8, 6, 4, 20, 20, false, 6, 3, false, true, numKills, numDefeats, experiencePoints, false, items, itemEquipped, new Spell[] { new Spell ("BLAZE", 1), null, null, null }, TexturesMonica, TexturesMonica_Portrait);
                    break;
            }

            if (ItemEquipped != null)
                for (int i = 0; i < MAXNUMBER_ITEMS; i++)
                    if (ItemEquipped[i])
                    {
                        ItemEquipped[i] = false;
                        Equip(i);
                    }

            if (_level == level - levelToDisplay + 1)
                Promote(promotedStatus);

            while (_level < level)
            {
                NextLevel();
                if (_level == level - levelToDisplay + 1)
                    Promote(promotedStatus);
            }

            _hitPoints = _maxHitPoints;
            _magicPoints = _maxMagicPoints;
        }

        public void Init(String name, String charClass, int level, int levelToDisplay, int hitPoints, int maxHitPoints, int attackPoints, int defensePoints, int magicPoints, int maxMagicPoints, bool mustSurvive, int movePoints, int agility, bool flying, bool alive, int numKills, int numDefeats, int experiencePoints, bool hasSpecialAttack, Item[] items, bool[] itemEquipped, Spell[] magicSpells, Texture2D[] texture, Texture2D portrait)
        {
            Init(name, charClass, level, levelToDisplay, PromotedStatuses.No, hitPoints, maxHitPoints, attackPoints, defensePoints, magicPoints, maxMagicPoints, mustSurvive, movePoints, agility, flying, alive, numKills, numDefeats, experiencePoints, 1, 1, hasSpecialAttack, items, itemEquipped, magicSpells, texture, portrait);
        }

        // *** constructor temporarily for enemy party members, later on also for player party members
        public Character(int id, CharacterPointer.Sides side)
        {
            bool[] noItemEquipped = new bool[4] { false, false, false, false } ;

            switch (side)
            {
                case CharacterPointer.Sides.CPU_Opponents:
                    switch ((Enemies)id)
                    {
                        case Enemies.Ninja:
                            Init("NINJA", 4, 12, 15, 5, 0, false, false, 5, 23, false, 2, 3, false, false, false, false, false, false, false, false, false, false, false, 38, new Item[] { new Item("Medical", " Herb"), null, null, null }, noItemEquipped, null, TexturesNinja, null);
                            break;

                        case Enemies.Banana:
                            Init("BANANA", 2, 10, 12, 3, 0, false, false, 6, 18, false, 1, 1, false, false, false, false, false, false, false, false, false, false, false, 16, null, noItemEquipped, null, TexturesBanana, null);
                            break;

                        case Enemies.Witch:
                            Init("WITCH", 5, 20, 8, 2, 12, false, false, 6, 25, false, 1, 1, false, false, false, false, false, false, false, false, false, false, false, 42, null, noItemEquipped, new Spell[] { new Spell("BLAZE", 2), null, null, null }, TexturesWitch, null);
                            break;

                        case Enemies.Samurai:
                            Init("SAMURAI", 6, 24, 21, 7, 0, false, false, 5, 30, false, 2, 3, false, false, false, false, false, false, false, false, false, false, false, 72, null, noItemEquipped, null, TexturesSamurai, null);
                            break;

                        case Enemies.Stanes:
                            Init("STANES", 7, 28, 22, 7, 0, false, false, 5, 32, false, 2, 3, false, false, false, false, false, false, false, false, false, false, false, 72, new Item[] { new Item("Medical", " Herb"), null, null, null }, noItemEquipped, null, TexturesStanes, null);
                            break;

                        case Enemies.Russell:
                            Init("RUSSELL", 8, 32, 24, 8, 0, false, true, 6, 34, false, 2, 3, false, false, false, false, false, false, false, false, false, false, false, 72, null, noItemEquipped, null, TexturesRussell, null);
                            break;
                    }
                    // ***
                    break;

                case CharacterPointer.Sides.Player:
                    // ***
                    break;
            }
        }

        // init for enemy party members
        public void Init(String name, int level, int maxHitPoints, int attackPoints, int defensePoints, int maxMagicPoints, bool mustSurvive, bool boss, int movePoints, int agility, bool flying, int minAttackRange, int maxAttackRange, bool susceptibleFire, bool susceptibleIce, bool resistantFire, bool resistantIce, bool canPoison, bool canDispel, bool canMuddle, bool canSlow, bool canStun, bool canSleep, bool hasSpecialAttack, int gold, Item[] items, bool[] itemEquipped, Spell[] magicSpells, Texture2D[] texture, Texture2D portrait)
        {
            Init(name, "", level, level, PromotedStatuses.No, maxHitPoints, maxHitPoints, attackPoints, defensePoints, maxMagicPoints, maxMagicPoints, mustSurvive, boss, movePoints, agility, flying, true, 0, 0, 0, minAttackRange, maxAttackRange, susceptibleFire, susceptibleIce, resistantFire, resistantIce, canPoison, canDispel, canMuddle, canSlow, canStun, canSleep, hasSpecialAttack, gold, items, itemEquipped, magicSpells, texture, portrait);
        }

        // general constructor
        public Character(String name, String charClass, int level, int levelToDisplay, PromotedStatuses promotedStatus, int hitPoints, int maxHitPoints, int attackPoints, int defensePoints, int magicPoints, int maxMagicPoints, bool mustSurvive, bool boss, int movePoints, int agility, bool flying, bool alive, int numKills, int numDefeats, int experiencePoints, int minAttackRange, int maxAttackRange, bool susceptibleFire, bool susceptibleIce, bool resistantFire, bool resistantIce, bool canPoison, bool canDispel, bool canMuddle, bool canSlow, bool canStun, bool canSleep, bool hasSpecialAttack, int gold, Item[] items, bool[] itemEquipped, Spell[] magicSpells, Texture2D[] texture, Texture2D portrait)
        {
            Init(name, charClass, level, levelToDisplay, promotedStatus, hitPoints, maxHitPoints, attackPoints, defensePoints, magicPoints, maxMagicPoints, mustSurvive, boss, movePoints, agility, flying, alive, numKills, numDefeats, experiencePoints, minAttackRange, maxAttackRange, susceptibleFire, susceptibleIce, resistantFire, resistantIce, canPoison, canDispel, canMuddle, canSlow, canStun, canSleep, hasSpecialAttack, gold, items, itemEquipped, magicSpells, texture, portrait);
        }

        public void Init(String name, String charClass, int level, int levelToDisplay, PromotedStatuses promotedStatus, int hitPoints, int maxHitPoints, int attackPoints, int defensePoints, int magicPoints, int maxMagicPoints, bool mustSurvive, bool boss, int movePoints, int agility, bool flying, bool alive, int numKills, int numDefeats, int experiencePoints, int minAttackRange, int maxAttackRange, bool susceptibleFire, bool susceptibleIce, bool resistantFire, bool resistantIce, bool canPoison, bool canDispel, bool canMuddle, bool canSlow, bool canStun, bool canSleep, bool hasSpecialAttack, int gold, Item[] items, bool[] itemEquipped, Spell[] magicSpells, Texture2D[] texture, Texture2D portrait)
        {
            _name = name;
            _charClass = charClass;
            _level = level;
            _levelToDisplay = levelToDisplay;
            _promotedStatus = promotedStatus;
            _hitPoints = hitPoints;
            _maxHitPoints = maxHitPoints;
            _attackPoints = attackPoints;
            _defensePoints = defensePoints;
            _magicPoints = magicPoints;
            _maxMagicPoints = maxMagicPoints;
            _mustSurvive = mustSurvive;
            _boss = boss;
            _movePoints = movePoints;
            _agility = agility;
            _flying = flying;
            _alive = alive;
            _visible = true;
            _portrait = portrait;
            _numKills = numKills;
            _numDefeats = numDefeats;
            _experiencePoints = experiencePoints;
            _minAttackRange = minAttackRange;
            _maxAttackRange = maxAttackRange;
            _susceptibleFire = susceptibleFire;
            _susceptibleIce = susceptibleIce;
            _resistantFire = resistantFire;
            _resistantIce = resistantIce;
            _canPoison = canPoison;
            _canDispel = canDispel;
            _canMuddle = canMuddle;
            _canSlow = canSlow;
            _canStun = canStun;
            _canSleep = canSleep;
            _hasSpecialAttack = hasSpecialAttack;
            _gold = gold;
            _asleep = 0;
            _poisoned = false;
            _cursed = false;
            _stunned = 0;
            _silenced = 0;
            _confused = 0;
            _boosted = 0;
            _attackBoosted = 0;
            _agilityBoostedBy = 0;
            _defenseBoostedBy = 0;
            _attackBoostedBy = 0;
            Items = items;
            ItemEquipped = itemEquipped;
            MagicSpells = magicSpells;

            for (int i = 0; i < 8; i++)
                _texture[i] = texture[i];

            PositionStatus = PositionStatuses.LooksDown1;
        }

        // constructor for player party members
        public Character(String name, String charClass, int level, int levelToDisplay, PromotedStatuses promotedStatus, int hitPoints, int maxHitPoints, int attackPoints, int defensePoints, int magicPoints, int maxMagicPoints, bool mustSurvive, int movePoints, int agility, bool flying, bool alive, int numKills, int numDefeats, int experiencePoints, int minAttackRange, int maxAttackRange, bool hasSpecialAttack, Item[] items, bool[] itemEquipped, Spell[] magicSpells, Texture2D[] texture, Texture2D portrait)
        {
            Init (name, charClass, level, levelToDisplay, promotedStatus, hitPoints, maxHitPoints, attackPoints, defensePoints, magicPoints, maxMagicPoints, mustSurvive, movePoints, agility, flying, alive, numKills, numDefeats, experiencePoints, minAttackRange, maxAttackRange, hasSpecialAttack, items, itemEquipped, magicSpells, texture, portrait);
        }

        public void Init (String name, String charClass, int level, int levelToDisplay, PromotedStatuses promotedStatus, int hitPoints, int maxHitPoints, int attackPoints, int defensePoints, int magicPoints, int maxMagicPoints, bool mustSurvive, int movePoints, int agility, bool flying, bool alive, int numKills, int numDefeats, int experiencePoints, int minAttackRange, int maxAttackRange, bool hasSpecialAttack, Item[] items, bool[] itemEquipped, Spell[] magicSpells, Texture2D[] texture, Texture2D portrait)
        {
            _name = name;
            _charClass = charClass;
            _level = level;
            _levelToDisplay = levelToDisplay;
            _promotedStatus = promotedStatus;
            _hitPoints = hitPoints;
            _maxHitPoints = maxHitPoints;
            _attackPoints = attackPoints;
            _defensePoints = defensePoints;
            _magicPoints = magicPoints;
            _maxMagicPoints = maxMagicPoints;
            _mustSurvive = mustSurvive;
            _boss = false;
            _movePoints = movePoints;
            _agility = agility;
            _flying = flying;
            _alive = alive;
            _visible = true;
            _portrait = portrait;
            _numKills = numKills;
            _numDefeats = numDefeats;
            _experiencePoints = experiencePoints;
            _minAttackRange = minAttackRange;
            _maxAttackRange = maxAttackRange;
            _susceptibleFire = false;
            _susceptibleIce = false;
            _resistantFire = false;
            _resistantIce = false;
            _canPoison = false;
            _canDispel = false;
            _canMuddle = false;
            _canSlow = false;
            _canStun = false;
            _canSleep = false;
            _gold = 0;
            _asleep = 0;
            _poisoned = false;
            _cursed = false;
            _stunned = 0;
            _silenced = 0;
            _confused = 0;
            _boosted = 0;
            _attackBoosted = 0;
            _agilityBoostedBy = 0;
            _defenseBoostedBy = 0;
            _attackBoostedBy = 0;
            _hasSpecialAttack = hasSpecialAttack;
            Items = items;
            ItemEquipped = itemEquipped;
            MagicSpells = magicSpells;

            for (int i = 0; i < 8; i++)
                _texture[i] = texture[i];

            PositionStatus = PositionStatuses.LooksDown1;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            DrawAt(spriteBatch, position * new Vector2(Map.TILESIZEX, Map.TILESIZEY) + new Vector2(OFFSETX, OFFSETY), _visible);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, byte opacity1)
        {
            DrawAt(spriteBatch, position * new Vector2(Map.TILESIZEX, Map.TILESIZEY) + new Vector2(OFFSETX, OFFSETY), _visible, opacity1);
        }

        public void DrawAt(SpriteBatch spriteBatch, Vector2 posXY)
        {
            DrawAt(spriteBatch, posXY, _visible);
        }

        public void DrawAt(SpriteBatch spriteBatch, Vector2 posXY, bool visible)
        {
            DrawAt(spriteBatch, posXY, visible, 255);
        }

        public void DrawAt(SpriteBatch spriteBatch, Vector2 posXY, bool visible, byte opacity1)
        {
            if (visible)
                spriteBatch.Draw(_texture[(int)_positionStatus], posXY, new Color(opacity1, opacity1, opacity1, opacity1));
        }

        public void DrawPortraitAt(SpriteBatch spriteBatch, Vector2 posXY)
        {
            spriteBatch.Draw(_portrait, posXY, Color.White);
        }

        public int GetNextEquippableWeapon(int skip)
        {
            if (Items != null)
                for (int i = 0; i < MAXNUMBER_WEAPONS; i++)
                    if (Items[i] != null && Items[i].Type == Item.Types.Weapon && Items[i].CanBeEquipped(CharClass))
                    {
                        if (skip == 0)
                            return i;
                        else
                            skip--;
                    }

            return -1; // there's none
        }

        public int GetNextEquippableRing(int skip)
        {
            if (Items != null)
                for (int i = 0; i < MAXNUMBER_RINGS; i++)
                    if (Items[i] != null && Items[i].Type == Item.Types.Ring)
                    {
                        if (skip == 0)
                            return i;
                        else
                            skip--;
                    }

            return -1;
        }

        public int GetEquippedWeapon()
        {
            if (Items != null)
                for (int i = 0; i < MAXNUMBER_WEAPONS; i++)
                    if (Items[i] != null && Items[i].Type == Item.Types.Weapon && ItemEquipped[i] == true)
                        return i;

            return -1; // there's none
        }

        public int GetEquippedRing()
        {
            if (Items != null)
                for (int i = 0; i < MAXNUMBER_RINGS; i++)
                    if (Items[i] != null && Items[i].Type == Item.Types.Ring && ItemEquipped[i] == true)
                        return i;

            return -1; // there's none
        }

        public void Unequip(int whichOne)
        {
            if (whichOne != -1 && ItemEquipped[whichOne] == true)
            {
                ItemEquipped[whichOne] = false;
                _attackPoints -= Items[whichOne].AttackPoints;
                _defensePoints -= Items[whichOne].DefensePoints;
                _agility -= Items[whichOne].Agility;
                _movePoints -= Items[whichOne].MovePoints;
                _minAttackRange = 1;
                _maxAttackRange = 1;
            }
        }

        public void Equip(int whichOne)
        {
            if (whichOne != -1 && ItemEquipped[whichOne] == false)
            {
                ItemEquipped[whichOne] = true;
                _attackPoints += Items[whichOne].AttackPoints;
                _defensePoints += Items[whichOne].DefensePoints;
                _agility += Items[whichOne].Agility;
                _movePoints += Items[whichOne].MovePoints;
                if (Items[whichOne].Type == Item.Types.Weapon)
                {
                    _minAttackRange = Items[whichOne].MinAttackRange;
                    _maxAttackRange = Items[whichOne].MaxAttackRange;
                }
            }
        }

        public int NextSpell()
        {
            int i;

            for (i = 0; i < MAXNUMBER_SPELLS; i++)
                if (MagicSpells[i] == null)
                    return i;

            return -1; // no free spell slot
        }

        public LevelUpMessage NextLevel()
        {
            LevelUpMessage returnValue = new LevelUpMessage(LevelUpMessage.Messages.None, 0);

            _level++;
            _levelToDisplay++;
            _oldMaxHitPoints = _maxHitPoints;
            _oldMaxMagicPoints = _maxMagicPoints;
            _oldAttackPoints = _attackPoints;
            _oldDefensePoints = _defensePoints;
            _oldAgility = _agility;

            // *** here comes the code that determines what skills are improved for each character and for each level
            switch (_name)
            {
                case "JACOB":
                    _maxHitPoints += 1;
                    if (_level % 2 == 0) _maxMagicPoints += 1;
                    _attackPoints += 1;
                    _defensePoints += 1;
                    _agility += 1;
                    break;

                case "CARYN":
                    if (_level % 2 == 0) _maxHitPoints += 1;
                    _maxMagicPoints += 1;
                    if (_level % 2 == 0) _attackPoints += 1;
                    if (_level % 2 == 0) _defensePoints += 1;
                    _agility += 1;
                    break;

                case "TASSI":
                    _maxHitPoints += 1;
                    _maxMagicPoints += 0;
                    _attackPoints += 1;
                    if (_level % 2 == 0) _attackPoints += 1;
                    _defensePoints += 1;
                    if (_level % 2 == 0) _defensePoints += 1;
                    _agility += 1;
                    break;

                case "EVA":
                    _maxHitPoints += 1;
                    _maxMagicPoints += 0;
                    _attackPoints += 1;
                    if (_level % 2 == 0) _attackPoints += 1;
                    if (_level % 2 == 0) _defensePoints += 1;
                    _agility += 1;
                    break;

                case "HANS":
                    _maxHitPoints += 1;
                    _maxMagicPoints += 0;
                    _attackPoints += 1;
                    if (_level % 3 == 0) _attackPoints += 2;
                    _defensePoints += 1;
                    if (_level % 3 == 0) _defensePoints += 1;
                    _agility += 1;
                    break;

                case "MONICA":
                    if (_level % 2 == 0) _maxHitPoints += 1;
                    _maxMagicPoints += 1;
                    if (_level % 2 == 0) _attackPoints += 1;
                    if (_level % 2 == 0) _defensePoints += 1;
                    _agility += 1;
                    if (_level == 4) MagicSpells[0].Level = 2;
                    break;
            }

            return returnValue;
        }

        public bool CanCastHealSpell()
        {
            return CanCastSpellWithType(Spell.Types.Heal);
        }

        public bool CanCastAttackSpell()
        {
            return CanCastSpellWithType(Spell.Types.Attack);
        }

        public bool CanCastSpellWithType(Spell.Types type)
        {
            if (MagicSpells != null)
                for (int i = 0; i < MAXNUMBER_SPELLS; i++)
                    if (MagicSpells[i] != null && MagicSpells[i].Type == type && MagicSpells[i].MagicPoints[0] <= MagicPoints)
                        return true;

            return false;
        }

        public bool HasHealItem()
        {
            return HasItemWithType(Spell.Types.Heal);
        }

        public bool HasAttackItem()
        {
            return HasItemWithType(Spell.Types.Attack);
        }

        public bool HasItemWithType(Spell.Types type)
        {
            if (Items != null)
                for (int i = 0; i < MAXNUMBER_ITEMS; i++)
                    if (Items[i] != null && Items[i].MagicSpell != null && Items[i].MagicSpell.Type == type)
                        return true;

            return false;
        }

        public bool CanUseHealMagic()
        {
            return CanCastHealSpell() || HasHealItem();
        }

        public bool CanUseAttackMagic()
        {
            return CanCastAttackSpell() || HasAttackItem();
        }

        public void RearrangeItems(int selectedItemNumber)
        {
            bool found = false;

            for (int i = MAXNUMBER_ITEMS - 1; !found && i > selectedItemNumber; i--)
                if (Items[i] != null)
                {
                    Items[selectedItemNumber] = Items[i];
                    Items[i] = null;
                    found = true;
                }
        }

        public void LookUp()
        {
            if ((int)_positionStatus % 2 == 0)
                _positionStatus = PositionStatuses.LooksUp1;
            else
                _positionStatus = PositionStatuses.LooksUp2;
        }

        public void LookDown()
        {
            if ((int)_positionStatus % 2 == 0)
                _positionStatus = PositionStatuses.LooksDown1;
            else
                _positionStatus = PositionStatuses.LooksDown2;
        }

        public void LookLeft()
        {
            if ((int)_positionStatus % 2 == 0)
                _positionStatus = PositionStatuses.LooksLeft1;
            else
                _positionStatus = PositionStatuses.LooksLeft2;
        }

        public void LookRight()
        {
            if ((int)_positionStatus % 2 == 0)
                _positionStatus = PositionStatuses.LooksRight1;
            else
                _positionStatus = PositionStatuses.LooksRight2;
        }

        public void RotateClockwise()
        {
            switch (_positionStatus)
            {
                case PositionStatuses.LooksDown1:
                case PositionStatuses.LooksDown2:
                    LookLeft();
                    break;

                case PositionStatuses.LooksLeft1:
                case PositionStatuses.LooksLeft2:
                    LookUp();
                    break;

                case PositionStatuses.LooksUp1:
                case PositionStatuses.LooksUp2:
                    LookRight();
                    break;

                case PositionStatuses.LooksRight1:
                case PositionStatuses.LooksRight2:
                    LookDown();
                    break;
            }
        }

        public int ItemToLose()
        {
            if (Items != null)
                for (int i = MAXNUMBER_ITEMS - 1; i >= 0; i--)
                    if (Items[i] != null && !ItemEquipped[i])
                        return i;

            return -1;
        }

        public int FreeItemSlot()
        {
            if (Items == null)
                Items = new Item[MAXNUMBER_ITEMS];

            for (int i = 0; i < MAXNUMBER_ITEMS; i++)
                if (Items[i] == null)
                    return i;

            return -1;
        }

        public void Regenerate()
        {
            HitPoints = MaxHitPoints;
            MagicPoints = MaxMagicPoints;
        }

        public void UnBoost()
        {
            _agility -= _agilityBoostedBy;
            _agilityBoostedBy = 0;
            _defensePoints -= _defenseBoostedBy;
            _defenseBoostedBy = 0;
        }

        public void UnAttackBoost()
        {
            _attackPoints -= _attackBoostedBy;
            _attackBoostedBy = 0;
        }

        public void Promote(PromotedStatuses newPromotedStatus)
        {
            if (_promotedStatus == PromotedStatuses.YesRegular
                || _promotedStatus == PromotedStatuses.YesAlternative)
                return;         // don't promote twice

            switch (newPromotedStatus)
            {
                case PromotedStatuses.YesRegular:
                    switch (_charClass)
                    {
                        case "ACHR":
                            _charClass = "SNIP";
                            _movePoints += 1;
                            break;

                        case "BDMN":
                            _charClass = "BDBT";
                            break;

                        case "KNTE":
                            _charClass = "PLDN";
                            break;

                        case "MAGE":
                            _charClass = "WIZ";
                            break;

                        case "PHNK":
                            _charClass = "PHNX";
                            break;

                        case "PRST":
                            _charClass = "VICR";
                            break;

                        case "RNGR":
                            _charClass = "BWNT";
                            break;

                        case "SDMN":
                            _charClass = "HERO";
                            break;

                        case "THIF":
                            _charClass = "NINJ";
                            break;

                        case "TORT":
                            _charClass = "MNST";
                            _movePoints += 2;
                            _flying = true;
                            _hasSpecialAttack = true;
                            break;

                        case "WARR":
                            _charClass = "GLDT";
                            break;

                        case "WFMN":
                            _charClass = "WFBN";
                            break;
                    }
                    _levelToDisplay = 1;
                    break;

                case PromotedStatuses.YesAlternative:
                    switch (_charClass)
                    {
                        case "ACHR":
                            _charClass = "BRGN";
                            break;

                        case "KNTE":
                            _charClass = "PGNT";
                            break;

                        case "MAGE":
                            _charClass = "SORC";
                            // *** new spells
                            break;

                        case "PRST":
                            _charClass = "MMNK";
                            if (Items != null && ItemEquipped != null)
                                for (int i = 0; i < MAXNUMBER_ITEMS; i++)
                                    if (Items[i].Type == Item.Types.Weapon
                                        && ItemEquipped[i])
                                        Unequip(i);
                            break;

                        case "WARR":
                            _charClass = "BRN";
                            _movePoints += 1;
                            break;
                    }
                    _levelToDisplay = 1;
                    break;
            }

            _promotedStatus = newPromotedStatus;
        }
    }
}
