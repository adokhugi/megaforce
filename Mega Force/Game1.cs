using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WindowsGame2
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        const bool DEBUG_SKIPENEMY = false;
        const int KEY_DELAY = 30;
        const int MOVEX = 5;
        const int MOVEY = 4;
        const int DEFEAT_DISPLAY_WAIT_DURATION = 8 * 4;
        const int DEFEAT_DISPLAY_EXPLOSION_STEP_DURATION = 3;
        const int DEFEAT_DISPLAY_EXPLOSION_NUMBER_STEPS = 3;
        const int DEFEAT_DISPLAY_EXPLOSION_DURATION = DEFEAT_DISPLAY_EXPLOSION_STEP_DURATION * DEFEAT_DISPLAY_EXPLOSION_NUMBER_STEPS;
        const int SELECTIONBOX_COLUMN1_X = 0;
        const int SELECTIONBOX_COLUMN2_X = 40;
        const int SELECTIONBOX_COLUMN3_X = 80;
        const int SELECTIONBOX_ROW1_Y = 0;
        const int SELECTIONBOX_ROW2_Y = 24;
        const int SELECTIONBOX_ROW3_Y = 48;
        const int POISON_EFFECT = 2;
        const int TEXTMESSAGE_MAXNUMBERROWS = 20;
        const int MEMBERMENU_NUMCHARSPERPAGE = 5;
        const int BLINK_DELAY = 6;
        const int TEXTBOX_WIDTH = 30;
        const int ENOUGH_MONEY_TO_CONTINUE_ONCE = 500;

        // 2024.12.02 introduced constants
        public const int PREFERREDBACKBUFFERWIDTH = 1024;
        public const int PREFERREDBACKBUFFERHEIGHT = 768;

        enum GameModes
        {
            // battlefield modes
            SelectionBarMode,
            PlayerMoveMode,
            SelectionBarTransitionMode,
            PlayerMoveTransitionMode,
            PlayerMovingMode,
            BattleMenuMode,
            PlayerMovingInSelectionBarTransitionMode,
            TransitionInPlayerMoveTransitionMode,
            TransitionInSelectionBarMode,
            LargeCharacterStatsMode,
            SmallCharacterStatsMode,
            AttackMenuMode,
            TransitionInAttackMenuMode,
            SelectionBarTransitionOutAttackMenuMode,
            GeneralMenuMode,
            ItemMenuMode,
            GeneralMenuMoveOutMode,
            GeneralMenuMoveInMode,
            BattleMenuMoveOutMode,
            BattleMenuMoveInMode,
            ItemMenuMoveOutMode,
            ItemMenuMoveInMode,
            ItemMagicSelectionMenuMode,
            ItemMagicSelectionMenuMoveInMode,
            ItemMagicSelectionMenuMoveOutMode,
            DisplayTextMessageMode,
            DisplayTextMessageMoveOutMode,
            DropItemMode,
            GiveItemMode,
            SelectionBarTransitionInGiveItemMode,
            SelectionBarTransitionOutGiveItemMode,
            SwapItemMode,
            SwapItemMoveInMode,
            SwapItemMoveOutMode,
            EquipWeaponMode,
            EquipWeaponMoveInMode,
            EquipWeaponMoveOutMode,
            EquipRingMode,
            EquipRingMoveInMode,
            EquipRingMoveOutMode,
            PlayerBattleMode,
            DisplayDefeatedCharactersMode,
            NextCharacterAfterSleepMode,
            PlayerBattleDisplayTextMessageMode,
            BattleDisplayTextMessageMoveOutMode,
            MagicLevelSelectionMode,
            EnemyMoveTransitionMode,
            TransitionInEnemyMoveTransitionMode,
            EnemyMoveMode,
            EnemyMovingMode,
            EnemyBattleMode,
            EnemyBattleDisplayTextMessageMode,
            EnemyAttackMode,
            TransitionInEnemyAttackMode,
            HealMenuMode,
            TransitionInHealMenuMode,
            EndTurnOfCharacterAndNextCharacterMode,
            BattleLost1Mode,
            BattleLost2Mode,
            BattleWonMode,
            PlayerAutoMoveMode,
            PlayerAutoAttackMode,
            EgressMode,
            BattleFieldFadeOutMode,
            BattleFieldFadeInMode,
            InitializeBattle1Mode,
            InitializeBattle2Mode,
            EndOfBigBattleMode,
            MemberMenuMode,
            // titlescreen and story-telling modes
            TitleScreenMode,
            StoryMode,
            AfterTitleScreenMode
        }

        enum BattleModeStates
        {
            InitialMessage,
            SubtractMagicPoints,
            CalculateDamage,
            DisplayDamageAndCalculateExperiencePointsAndGold,
            UpdateItemStatusAndCheckSecondAttack,
            CounterAttackInitialMessage,
            CounterAttackCalculateDamage,
            CounterAttackDisplayDamage,
            CounterAttackDisplayExperiencePointsAndGold,
            DisplayExperiencePointsAndGold,
            CheckIfAnyCharacterIsDefeated,
            NextCharacter
        }

        enum BattleMoves
        {
            Attack,
            MagicAttack,
            MagicHeal,
            ItemMagicAttack,
            ItemMagicHeal,
            Stay
        }

        enum PartySet
        {
            NotSet,
            SetOnce,
            SetForGood
        }

        enum EndTurnOfCharacterAndNextCharacterModeStates
        {
            Poisoned,
            PoisonedDefeated,
            Silenced,
            Confused,
            Boosted,
            AttackBoosted,
            NextCharacter
        }

        double gameTimeAtLastBlink;
        bool blinkDelayReached;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        int gold;
        int speed;
        Map map;
        Party party;
        Party enemies;
        CharacterPointer characterPointer;
        SelectionBar selectionBar;
        SelectionBar selectionBar2;
        SelectionBar selectionBar3;
        Vector2 tempSelectionBarPosition;
        KeyboardState oldState;
        GameModes GameMode;
        GameModes returnGameMode;
        float backUpPosition;
        Vector2[] mapPositionPlayerMoveMode = new Vector2[Party.MAXPARTYMEMBERS];
        Vector2[] mapPositionEnemyMoveMode = new Vector2[Party.MAXPARTYMEMBERS];
        Vector2 oldSelectionBarPosition;
        Vector2 oldMapPosition;
        int keyHoldCounter;
        //bool keyRelieved;
        Vector2 delayPosition;
        BattleMenu battleMenu;
        GeneralMenu generalMenu;
        ItemMenu itemMenu;
        ItemMagicSelectionMenu itemMagicSelectionMenu;
        ItemMagicSelectionMenu swapMenu;
        ItemMagicSelectionMenu equipWeaponMenu;
        ItemMagicSelectionMenu equipRingMenu;
        bool lockDraw;
        bool lockInput;
        Box landEffectBox;
        Box characterStatsBox;
        Box largeCharacterStatsMainBox;
        Box portraitBox;
        Box largeCharacterStatsKillsDefeatsBox;
        Box largeCharacterStatsGoldBox;
        Box captionBox;
        Box equipSpecsBox;
        Box textMessageBox;
        int textMessageLength;
        bool textMessageContinueFlag;
        Arrow arrowforward;
        Arrow arrowbackward;
        CharacterPointer whoseStatsToBeDisplayed;
        Font smallFont;
        Font italicFont;
        int skip;
        Vector2 tempMenuPosition;
        String[] textMessage = new string[TEXTMESSAGE_MAXNUMBERROWS];
        PositionInTextMessage positionInTextMessage;
        int textMessageDelay;
        bool waitForKeyPress;
        bool moveOut;
        Vector2 tempTextMessageBoxPosition;
        YesNoButtons yesNoButtons;
        Item nothing;
        int[] weapons = new int[4];
        int[] rings = new int[4];
        int backUpEquippedWeapon;
        int backUpEquippedRing;
        BattleModeStates battleModeState;
        Random random = new Random();
        int damage;
        float damageModifier;
        CharacterPointer[] targetsAttackedOrHealed = new CharacterPointer[Party.MAXPARTYMEMBERS];
        int numTargetsAttackedOrHealed;
        int currentTargetAttackedOrHealed; 
        int expEarned;
        int goldFound;
        int displayDefeatedCharactersModeState;
        Texture2D[] explosionTexture = new Texture2D[3];
        Texture2D spellLevelSelectedTexture;
        Texture2D spellLevelUnselectedTexture;
        int selectedMagicLevel;
        int selectedSpellNumber;
        int selectedItemNumber;
        int selectedWeaponNumber;
        int selectedRingNumber;
        int selectedSwapItemNumber;
        Redbar redbar;
        int currentStepNumber;
        BattleMoves enemyBattleMove;
        CharacterPointer target;
        int targetPosition;
        EndTurnOfCharacterAndNextCharacterModeStates endTurnOfCharacterAndNextCharacterModeState;
        int textMessageCounter;
        StatsBar statsBarHitPoints;
        StatsBar statsBarMagicPoints;
        bool secondAttack;
        byte fadingState;
        byte fadingSpeed;
        int turn;
        int memberMenuState_selectedMemberNumber;
        int memberMenuState_firstMemberNumberToDisplay;
        bool memberMenuState_displayDetails;
        Box memberMenuDisplayStatsBox;
        Box memberMenuCharacterSelectionBox;
        Spell selectedMagicSpell;
        bool spellHasNoEffect;
        bool battleWon;
        int progress;
        Story story;
        string story_character;
        Character hero;
        Character healer;
        Character warrior;
        Character archer;
        Character knight;
        Character mage;
        Item medicalHerb;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = PREFERREDBACKBUFFERWIDTH;
            graphics.PreferredBackBufferHeight = PREFERREDBACKBUFFERHEIGHT;
            graphics.IsFullScreen = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Map.Texture[(int)Map.DisplayStates.Mountain_Default] = this.Content.Load<Texture2D>("new_terrain_mountain");
            Map.Texture[(int)Map.DisplayStates.Forest_Default] = this.Content.Load<Texture2D>("new_terrain_forest");
            Map.Texture[(int)Map.DisplayStates.Grassland_Default] = this.Content.Load<Texture2D>("new_terrain_grassland");
            Map.Texture[(int)Map.DisplayStates.Hill_Default] = this.Content.Load<Texture2D>("new_terrain_hill");
            Map.Texture[(int)Map.DisplayStates.Sea_Default] = this.Content.Load<Texture2D>("new_terrain_sea");
            Map.Texture[(int)Map.DisplayStates.Path_Default] = this.Content.Load<Texture2D>("new_terrain_path");
            Map.Texture[(int)Map.DisplayStates.Wall_Default] = this.Content.Load<Texture2D>("new_terrain_wall");

            explosionTexture[0] = this.Content.Load<Texture2D>("shfo2_explosion_0");
            explosionTexture[1] = this.Content.Load<Texture2D>("shfo2_explosion_1");
            explosionTexture[2] = this.Content.Load<Texture2D>("shfo2_explosion_2");

            spellLevelSelectedTexture = this.Content.Load<Texture2D>("shfo2_spell-level-selection_selected");
            spellLevelUnselectedTexture = this.Content.Load<Texture2D>("shfo2_spell-level-selection_unselected");

            redbar = new Redbar();
            redbar.Texture = this.Content.Load<Texture2D>("shfo2_spell-level-selection_redbar");

            selectionBar = new SelectionBar();
            selectionBar.Texture = this.Content.Load<Texture2D>("shfo2_selectionbar");

            selectionBar2 = new SelectionBar();
            selectionBar2.Texture = this.Content.Load<Texture2D>("shfo2_selectionbar2");

            selectionBar3 = new SelectionBar();
            selectionBar3.Texture = this.Content.Load<Texture2D>("shfo2_selectionbar3");

            statsBarHitPoints = new StatsBar();
            statsBarHitPoints.TextureLeft = this.Content.Load<Texture2D>("shfo2_statsbar_left");
            statsBarHitPoints.TextureRight = this.Content.Load<Texture2D>("shfo2_statsbar_right");
            statsBarHitPoints.TextureMiddle[0] = this.Content.Load<Texture2D>("shfo2_statsbar_middle0");
            statsBarHitPoints.TextureMiddle[1] = this.Content.Load<Texture2D>("shfo2_statsbar_middle1");
            statsBarHitPoints.TextureMiddle[2] = this.Content.Load<Texture2D>("shfo2_statsbar_middle2");
            statsBarHitPoints.TextureMiddle[3] = this.Content.Load<Texture2D>("shfo2_statsbar_middle3");
            statsBarHitPoints.TextureMiddle[4] = this.Content.Load<Texture2D>("shfo2_statsbar_middle4");
            statsBarHitPoints.TextureMiddle[5] = this.Content.Load<Texture2D>("shfo2_statsbar_middle5");

            statsBarMagicPoints = new StatsBar();
            statsBarMagicPoints.TextureLeft = this.Content.Load<Texture2D>("shfo2_statsbar_left");
            statsBarMagicPoints.TextureRight = this.Content.Load<Texture2D>("shfo2_statsbar_right");
            statsBarMagicPoints.TextureMiddle[0] = this.Content.Load<Texture2D>("shfo2_statsbar_middle0");
            statsBarMagicPoints.TextureMiddle[1] = this.Content.Load<Texture2D>("shfo2_statsbar_middle1");
            statsBarMagicPoints.TextureMiddle[2] = this.Content.Load<Texture2D>("shfo2_statsbar_middle2");
            statsBarMagicPoints.TextureMiddle[3] = this.Content.Load<Texture2D>("shfo2_statsbar_middle3");
            statsBarMagicPoints.TextureMiddle[4] = this.Content.Load<Texture2D>("shfo2_statsbar_middle4");
            statsBarMagicPoints.TextureMiddle[5] = this.Content.Load<Texture2D>("shfo2_statsbar_middle5");

            battleMenu = new BattleMenu();
            battleMenu.Texture[0] = this.Content.Load<Texture2D>("new_battlemenu_default");
            battleMenu.Texture[1] = this.Content.Load<Texture2D>("new_battlemenu_stay");
            battleMenu.Texture[2] = this.Content.Load<Texture2D>("new_battlemenu_default");
            battleMenu.Texture[3] = this.Content.Load<Texture2D>("new_battlemenu_attack");
            battleMenu.Texture[4] = this.Content.Load<Texture2D>("new_battlemenu_default");
            battleMenu.Texture[5] = this.Content.Load<Texture2D>("new_battlemenu_magic");
            battleMenu.Texture[6] = this.Content.Load<Texture2D>("new_battlemenu_default");
            battleMenu.Texture[7] = this.Content.Load<Texture2D>("new_battlemenu_item");

            generalMenu = new GeneralMenu();
            generalMenu.Texture[0] = this.Content.Load<Texture2D>("shfo2_generalmenu_default");
            generalMenu.Texture[1] = this.Content.Load<Texture2D>("shfo2_generalmenu_quit");
            generalMenu.Texture[2] = this.Content.Load<Texture2D>("shfo2_generalmenu_default");
            generalMenu.Texture[3] = this.Content.Load<Texture2D>("shfo2_generalmenu_member");
            generalMenu.Texture[4] = this.Content.Load<Texture2D>("shfo2_generalmenu_default");
            generalMenu.Texture[5] = this.Content.Load<Texture2D>("shfo2_generalmenu_map");
            generalMenu.Texture[6] = this.Content.Load<Texture2D>("shfo2_generalmenu_default");
            generalMenu.Texture[7] = this.Content.Load<Texture2D>("shfo2_generalmenu_speed");

            itemMenu = new ItemMenu();
            itemMenu.Texture[0] = this.Content.Load<Texture2D>("shfo2_itemmenu_default");
            itemMenu.Texture[1] = this.Content.Load<Texture2D>("shfo2_itemmenu_drop");
            itemMenu.Texture[2] = this.Content.Load<Texture2D>("shfo2_itemmenu_default");
            itemMenu.Texture[3] = this.Content.Load<Texture2D>("shfo2_itemmenu_use");
            itemMenu.Texture[4] = this.Content.Load<Texture2D>("shfo2_itemmenu_default");
            itemMenu.Texture[5] = this.Content.Load<Texture2D>("shfo2_itemmenu_give");
            itemMenu.Texture[6] = this.Content.Load<Texture2D>("shfo2_itemmenu_default");
            itemMenu.Texture[7] = this.Content.Load<Texture2D>("shfo2_itemmenu_equip");

            itemMagicSelectionMenu = new ItemMagicSelectionMenu();
            itemMagicSelectionMenu.Texture = this.Content.Load<Texture2D>("shfo2_itemmagicselectionmenu");
            swapMenu = new ItemMagicSelectionMenu();
            swapMenu.Texture = this.Content.Load<Texture2D>("shfo2_itemmagicselectionmenu");
            equipWeaponMenu = new ItemMagicSelectionMenu();
            equipWeaponMenu.Texture = this.Content.Load<Texture2D>("shfo2_itemmagicselectionmenu");
            equipRingMenu = new ItemMagicSelectionMenu();
            equipRingMenu.Texture = this.Content.Load<Texture2D>("shfo2_itemmagicselectionmenu");

            Box.Texture_TopLeft = this.Content.Load<Texture2D>("shfo2_box_topleft");
            Box.Texture_TopMiddle = this.Content.Load<Texture2D>("shfo2_box_topmiddle");
            Box.Texture_TopRight = this.Content.Load<Texture2D>("shfo2_box_topright");
            Box.Texture_MiddleLeft = this.Content.Load<Texture2D>("shfo2_box_middleleft");
            Box.Texture_MiddleMiddle = this.Content.Load<Texture2D>("shfo2_box_middlemiddle");
            Box.Texture_MiddleRight = this.Content.Load<Texture2D>("shfo2_box_middleright");
            Box.Texture_BottomLeft = this.Content.Load<Texture2D>("shfo2_box_bottomleft");
            Box.Texture_BottomMiddle = this.Content.Load<Texture2D>("shfo2_box_bottommiddle");
            Box.Texture_BottomRight = this.Content.Load<Texture2D>("shfo2_box_bottomright");

            landEffectBox = new Box(new Vector2(160, 80));
            landEffectBox.Position = new Vector2(30, 30);
            characterStatsBox = new Box();
            largeCharacterStatsMainBox = new Box();
            portraitBox = new Box();
            portraitBox.OverrideTexture_TopMiddle = this.Content.Load<Texture2D>("shfo2_box_black_topmiddle");
            portraitBox.OverrideTexture_MiddleLeft = this.Content.Load<Texture2D>("shfo2_box_black_middleleft");
            portraitBox.OverrideTexture_MiddleMiddle = this.Content.Load<Texture2D>("shfo2_box_black_middlemiddle");
            portraitBox.OverrideTexture_MiddleRight = this.Content.Load<Texture2D>("shfo2_box_black_middleright");
            portraitBox.OverrideTexture_BottomMiddle = this.Content.Load<Texture2D>("shfo2_box_black_bottommiddle");
            largeCharacterStatsKillsDefeatsBox = new Box();
            largeCharacterStatsGoldBox = new Box();
            captionBox = new Box();
            equipSpecsBox = new Box();
            textMessageBox = new Box(new Vector2(578, 128));
            memberMenuDisplayStatsBox = new Box();
            memberMenuCharacterSelectionBox = new Box();

            arrowforward = new Arrow();
            arrowforward.Texture = this.Content.Load<Texture2D>("shfo2_arrowforward");
            arrowbackward = new Arrow();
            arrowbackward.Texture = this.Content.Load<Texture2D>("shfo2_arrowbackward");

            // 2024.12.02 introduced constants
            yesNoButtons = new YesNoButtons(new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 2 * 60 - 40) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 30 - textMessageBox.Size.Y - 40));
            yesNoButtons.TextureYes[0] = this.Content.Load<Texture2D>("shfo2_buttons_yes1");
            yesNoButtons.TextureYes[1] = this.Content.Load<Texture2D>("shfo2_buttons_yes2");
            yesNoButtons.TextureNo[0] = this.Content.Load<Texture2D>("shfo2_buttons_no1");
            yesNoButtons.TextureNo[1] = this.Content.Load<Texture2D>("shfo2_buttons_no2");

            smallFont = new Font(new Vector2(19, 16));
            smallFont.Characters['%'] = this.Content.Load<Texture2D>("shfo2_smallfont_%");
            smallFont.Characters[' '] = this.Content.Load<Texture2D>("shfo2_smallfont_space");
            smallFont.Characters['@'] = this.Content.Load<Texture2D>("shfo2_smallfont_equipped");
            smallFont.Characters['/'] = this.Content.Load<Texture2D>("shfo2_smallfont_slash");
            smallFont.Characters['^'] = this.Content.Load<Texture2D>("shfo2_smallfont_sword");
            smallFont.Characters['?'] = this.Content.Load<Texture2D>("shfo2_smallfont_questionmark");
            smallFont.Characters['0'] = this.Content.Load<Texture2D>("shfo2_smallfont_0");
            smallFont.Characters['1'] = this.Content.Load<Texture2D>("shfo2_smallfont_1");
            smallFont.Characters['2'] = this.Content.Load<Texture2D>("shfo2_smallfont_2");
            smallFont.Characters['3'] = this.Content.Load<Texture2D>("shfo2_smallfont_3");
            smallFont.Characters['4'] = this.Content.Load<Texture2D>("shfo2_smallfont_4");
            smallFont.Characters['5'] = this.Content.Load<Texture2D>("shfo2_smallfont_5");
            smallFont.Characters['6'] = this.Content.Load<Texture2D>("shfo2_smallfont_6");
            smallFont.Characters['7'] = this.Content.Load<Texture2D>("shfo2_smallfont_7");
            smallFont.Characters['8'] = this.Content.Load<Texture2D>("shfo2_smallfont_8");
            smallFont.Characters['9'] = this.Content.Load<Texture2D>("shfo2_smallfont_9");
            smallFont.Characters['A'] = this.Content.Load<Texture2D>("shfo2_smallfont_a");
            smallFont.Characters['B'] = this.Content.Load<Texture2D>("shfo2_smallfont_b");
            smallFont.Characters['C'] = this.Content.Load<Texture2D>("shfo2_smallfont_c");
            smallFont.Characters['D'] = this.Content.Load<Texture2D>("shfo2_smallfont_d");
            smallFont.Characters['E'] = this.Content.Load<Texture2D>("shfo2_smallfont_e");
            smallFont.Characters['F'] = this.Content.Load<Texture2D>("shfo2_smallfont_f");
            smallFont.Characters['G'] = this.Content.Load<Texture2D>("shfo2_smallfont_g");
            smallFont.Characters['H'] = this.Content.Load<Texture2D>("shfo2_smallfont_h");
            smallFont.Characters['I'] = this.Content.Load<Texture2D>("shfo2_smallfont_i");
            smallFont.Characters['J'] = this.Content.Load<Texture2D>("shfo2_smallfont_j");
            smallFont.Characters['K'] = this.Content.Load<Texture2D>("shfo2_smallfont_k");
            smallFont.Characters['L'] = this.Content.Load<Texture2D>("shfo2_smallfont_l");
            smallFont.Characters['M'] = this.Content.Load<Texture2D>("shfo2_smallfont_m");
            smallFont.Characters['N'] = this.Content.Load<Texture2D>("shfo2_smallfont_n");
            smallFont.Characters['O'] = this.Content.Load<Texture2D>("shfo2_smallfont_o");
            smallFont.Characters['P'] = this.Content.Load<Texture2D>("shfo2_smallfont_p");
            smallFont.Characters['Q'] = this.Content.Load<Texture2D>("shfo2_smallfont_q");
            smallFont.Characters['R'] = this.Content.Load<Texture2D>("shfo2_smallfont_r");
            smallFont.Characters['S'] = this.Content.Load<Texture2D>("shfo2_smallfont_s");
            smallFont.Characters['T'] = this.Content.Load<Texture2D>("shfo2_smallfont_t");
            smallFont.Characters['U'] = this.Content.Load<Texture2D>("shfo2_smallfont_u");
            smallFont.Characters['V'] = this.Content.Load<Texture2D>("shfo2_smallfont_v");
            smallFont.Characters['W'] = this.Content.Load<Texture2D>("shfo2_smallfont_w");
            smallFont.Characters['X'] = this.Content.Load<Texture2D>("shfo2_smallfont_x");
            smallFont.Characters['Y'] = this.Content.Load<Texture2D>("shfo2_smallfont_y");
            smallFont.Characters['Z'] = this.Content.Load<Texture2D>("shfo2_smallfont_z");
            smallFont.Characters['a'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_a");
            smallFont.Characters['b'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_b");
            smallFont.Characters['c'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_c");
            smallFont.Characters['d'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_d");
            smallFont.Characters['e'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_e");
            smallFont.Characters['f'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_f");
            smallFont.Characters['g'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_g");
            smallFont.Characters['h'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_h");
            smallFont.Characters['i'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_i");
            //smallFont.Characters['j'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_j");
            smallFont.Characters['k'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_k");
            smallFont.Characters['l'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_l");
            //smallFont.Characters['m'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_m");
            smallFont.Characters['n'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_n");
            smallFont.Characters['o'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_o");
            smallFont.Characters['p'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_p");
            smallFont.Characters['q'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_q");
            smallFont.Characters['r'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_r");
            smallFont.Characters['s'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_s");
            smallFont.Characters['t'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_t");
            smallFont.Characters['u'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_u");
            smallFont.Characters['v'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_v");
            smallFont.Characters['w'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_w");
            smallFont.Characters['x'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_x");
            smallFont.Characters['y'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_y");
            smallFont.Characters['z'] = this.Content.Load<Texture2D>("shfo2_smallfont_small_z");
            smallFont.Position = new Vector2(50, 50);

            italicFont = new Font(new Vector2(17, 33));
            italicFont.Characters[','] = this.Content.Load<Texture2D>("new_italicfont_comma");
            italicFont.Characters[':'] = this.Content.Load<Texture2D>("new_italicfont_colon");
            italicFont.Characters['.'] = this.Content.Load<Texture2D>("shfo2_italicfont_dot");
            italicFont.Characters['!'] = this.Content.Load<Texture2D>("shfo2_italicfont_exclamationmark");
            italicFont.Characters['?'] = this.Content.Load<Texture2D>("shfo2_italicfont_questionmark");
            italicFont.Characters['\''] = this.Content.Load<Texture2D>("shfo2_italicfont_highcomma");
            italicFont.Characters[' '] = this.Content.Load<Texture2D>("shfo2_italicfont_space");
            italicFont.Characters['0'] = this.Content.Load<Texture2D>("shfo2_italicfont_0");
            italicFont.Characters['1'] = this.Content.Load<Texture2D>("shfo2_italicfont_1");
            italicFont.Characters['2'] = this.Content.Load<Texture2D>("shfo2_italicfont_2");
            italicFont.Characters['3'] = this.Content.Load<Texture2D>("shfo2_italicfont_3");
            italicFont.Characters['4'] = this.Content.Load<Texture2D>("shfo2_italicfont_4");
            italicFont.Characters['5'] = this.Content.Load<Texture2D>("shfo2_italicfont_5");
            italicFont.Characters['6'] = this.Content.Load<Texture2D>("shfo2_italicfont_6");
            italicFont.Characters['7'] = this.Content.Load<Texture2D>("shfo2_italicfont_7");
            italicFont.Characters['8'] = this.Content.Load<Texture2D>("shfo2_italicfont_8");
            italicFont.Characters['9'] = this.Content.Load<Texture2D>("shfo2_italicfont_9");
            italicFont.Characters['A'] = this.Content.Load<Texture2D>("shfo2_italicfont_a");
            italicFont.Characters['B'] = this.Content.Load<Texture2D>("shfo2_italicfont_b");
            italicFont.Characters['C'] = this.Content.Load<Texture2D>("shfo2_italicfont_c");
            italicFont.Characters['D'] = this.Content.Load<Texture2D>("shfo2_italicfont_d");
            italicFont.Characters['E'] = this.Content.Load<Texture2D>("shfo2_italicfont_e");
            italicFont.Characters['F'] = this.Content.Load<Texture2D>("shfo2_italicfont_f");
            italicFont.Characters['G'] = this.Content.Load<Texture2D>("shfo2_italicfont_g");
            italicFont.Characters['H'] = this.Content.Load<Texture2D>("shfo2_italicfont_h");
            italicFont.Characters['I'] = this.Content.Load<Texture2D>("shfo2_italicfont_i");
            italicFont.Characters['J'] = this.Content.Load<Texture2D>("shfo2_italicfont_j");
            italicFont.Characters['K'] = this.Content.Load<Texture2D>("shfo2_italicfont_k");
            italicFont.Characters['L'] = this.Content.Load<Texture2D>("shfo2_italicfont_l");
            italicFont.Characters['M'] = this.Content.Load<Texture2D>("shfo2_italicfont_m");
            italicFont.Characters['N'] = this.Content.Load<Texture2D>("shfo2_italicfont_n");
            italicFont.Characters['O'] = this.Content.Load<Texture2D>("shfo2_italicfont_o");
            italicFont.Characters['P'] = this.Content.Load<Texture2D>("shfo2_italicfont_p");
            italicFont.Characters['Q'] = this.Content.Load<Texture2D>("new_italicfont_q");
            italicFont.Characters['R'] = this.Content.Load<Texture2D>("shfo2_italicfont_r");
            italicFont.Characters['S'] = this.Content.Load<Texture2D>("shfo2_italicfont_s");
            italicFont.Characters['T'] = this.Content.Load<Texture2D>("shfo2_italicfont_t");
            italicFont.Characters['U'] = this.Content.Load<Texture2D>("shfo2_italicfont_u");
            italicFont.Characters['V'] = this.Content.Load<Texture2D>("new_italicfont_v");
            italicFont.Characters['W'] = this.Content.Load<Texture2D>("shfo2_italicfont_w");
            italicFont.Characters['X'] = this.Content.Load<Texture2D>("shfo2_italicfont_x");
            italicFont.Characters['Y'] = this.Content.Load<Texture2D>("shfo2_italicfont_y");
            italicFont.Characters['Z'] = this.Content.Load<Texture2D>("shfo2_italicfont_z");
            italicFont.Characters['a'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_a");
            italicFont.Characters['b'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_b");
            italicFont.Characters['c'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_c");
            italicFont.Characters['d'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_d");
            italicFont.Characters['e'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_e");
            italicFont.Characters['f'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_f");
            italicFont.Characters['g'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_g");
            italicFont.Characters['h'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_h");
            italicFont.Characters['i'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_i");
            italicFont.Characters['j'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_j");
            italicFont.Characters['k'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_k");
            italicFont.Characters['l'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_l");
            italicFont.Characters['m'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_m");
            italicFont.Characters['n'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_n");
            italicFont.Characters['o'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_o");
            italicFont.Characters['p'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_p");
            italicFont.Characters['q'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_q");
            italicFont.Characters['r'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_r");
            italicFont.Characters['s'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_s");
            italicFont.Characters['t'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_t");
            italicFont.Characters['u'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_u");
            italicFont.Characters['v'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_v");
            italicFont.Characters['w'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_w");
            italicFont.Characters['x'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_x");
            italicFont.Characters['y'] = this.Content.Load<Texture2D>("shfo2_italicfont_small_y");
            italicFont.Characters['z'] = this.Content.Load<Texture2D>("new_italicfont_small_z");
            italicFont.CalcWidthOfCharacters();
            italicFont.WidthOfCharacter['\''] += 4;
            italicFont.WidthOfCharacter['A'] -= 1;
            italicFont.WidthOfCharacter['C'] -= 2;
            italicFont.WidthOfCharacter['D'] -= 1;
            italicFont.WidthOfCharacter['E'] -= 2;
            italicFont.WidthOfCharacter['G'] -= 2;
            italicFont.WidthOfCharacter['N'] -= 2;
            italicFont.WidthOfCharacter['R'] -= 1;
            italicFont.WidthOfCharacter['S'] -= 1;
            italicFont.WidthOfCharacter['T'] -= 2;
            italicFont.WidthOfCharacter['U'] -= 1;
            italicFont.WidthOfCharacter['Y'] -= 3;
            italicFont.WidthOfCharacter['W'] -= 5;
            italicFont.WidthOfCharacter['X'] -= 3;
            italicFont.WidthOfCharacter['l'] += 3;
            italicFont.WidthOfCharacter['q'] += 1;

            Spell.TextureSpellSelected = this.Content.Load<Texture2D>("shfo2_itemmagicselected");
            Spell.TextureBlaze = this.Content.Load<Texture2D>("new_spells_blaze");
            Spell.TextureHeal = this.Content.Load<Texture2D>("new_spells_heal");

            Item.TextureItemSelected = this.Content.Load<Texture2D>("shfo2_itemmagicselected");
            
            Item.TextureMedicalHerb = this.Content.Load<Texture2D>("new_items_medicalherb");

            Item.TextureNothing = this.Content.Load<Texture2D>("new_items_weapons_nothing");
            Item.TextureSword = this.Content.Load<Texture2D>("new_items_weapons_sword");
            Item.TextureStaff = this.Content.Load<Texture2D>("new_items_weapons_staff");
            Item.TextureArrow = this.Content.Load<Texture2D>("new_items_weapons_arrow");

            Item.TextureRing = this.Content.Load<Texture2D>("new_items_ring");

            Character.TexturesJacob[0] = this.Content.Load<Texture2D>("new_party_jacob");
            Character.TexturesJacob[1] = this.Content.Load<Texture2D>("new_party_jacob");
            Character.TexturesJacob[2] = this.Content.Load<Texture2D>("new_party_jacob");
            Character.TexturesJacob[3] = this.Content.Load<Texture2D>("new_party_jacob");
            Character.TexturesJacob[4] = this.Content.Load<Texture2D>("new_party_jacob");
            Character.TexturesJacob[5] = this.Content.Load<Texture2D>("new_party_jacob");
            Character.TexturesJacob[6] = this.Content.Load<Texture2D>("new_party_jacob");
            Character.TexturesJacob[7] = this.Content.Load<Texture2D>("new_party_jacob");
            Character.TexturesJacob_Portrait = this.Content.Load<Texture2D>("new_portrait_jacob");

            Character.TexturesCaryn[0] = this.Content.Load<Texture2D>("new_party_caryn");
            Character.TexturesCaryn[1] = this.Content.Load<Texture2D>("new_party_caryn");
            Character.TexturesCaryn[2] = this.Content.Load<Texture2D>("new_party_caryn");
            Character.TexturesCaryn[3] = this.Content.Load<Texture2D>("new_party_caryn");
            Character.TexturesCaryn[4] = this.Content.Load<Texture2D>("new_party_caryn");
            Character.TexturesCaryn[5] = this.Content.Load<Texture2D>("new_party_caryn");
            Character.TexturesCaryn[6] = this.Content.Load<Texture2D>("new_party_caryn");
            Character.TexturesCaryn[7] = this.Content.Load<Texture2D>("new_party_caryn");
            Character.TexturesCaryn_Portrait = this.Content.Load<Texture2D>("new_portrait_caryn");

            Character.TexturesTassi[0] = this.Content.Load<Texture2D>("new_party_tassi");
            Character.TexturesTassi[1] = this.Content.Load<Texture2D>("new_party_tassi");
            Character.TexturesTassi[2] = this.Content.Load<Texture2D>("new_party_tassi");
            Character.TexturesTassi[3] = this.Content.Load<Texture2D>("new_party_tassi");
            Character.TexturesTassi[4] = this.Content.Load<Texture2D>("new_party_tassi");
            Character.TexturesTassi[5] = this.Content.Load<Texture2D>("new_party_tassi");
            Character.TexturesTassi[6] = this.Content.Load<Texture2D>("new_party_tassi");
            Character.TexturesTassi[7] = this.Content.Load<Texture2D>("new_party_tassi");
            Character.TexturesTassi_Portrait = this.Content.Load<Texture2D>("new_portrait_tassi");

            Character.TexturesEva[0] = this.Content.Load<Texture2D>("new_party_eva");
            Character.TexturesEva[1] = this.Content.Load<Texture2D>("new_party_eva");
            Character.TexturesEva[2] = this.Content.Load<Texture2D>("new_party_eva");
            Character.TexturesEva[3] = this.Content.Load<Texture2D>("new_party_eva");
            Character.TexturesEva[4] = this.Content.Load<Texture2D>("new_party_eva");
            Character.TexturesEva[5] = this.Content.Load<Texture2D>("new_party_eva");
            Character.TexturesEva[6] = this.Content.Load<Texture2D>("new_party_eva");
            Character.TexturesEva[7] = this.Content.Load<Texture2D>("new_party_eva");
            Character.TexturesEva_Portrait = this.Content.Load<Texture2D>("new_portrait_eva");

            Character.TexturesHans[0] = this.Content.Load<Texture2D>("new_party_hans");
            Character.TexturesHans[1] = this.Content.Load<Texture2D>("new_party_hans");
            Character.TexturesHans[2] = this.Content.Load<Texture2D>("new_party_hans");
            Character.TexturesHans[3] = this.Content.Load<Texture2D>("new_party_hans");
            Character.TexturesHans[4] = this.Content.Load<Texture2D>("new_party_hans");
            Character.TexturesHans[5] = this.Content.Load<Texture2D>("new_party_hans");
            Character.TexturesHans[6] = this.Content.Load<Texture2D>("new_party_hans");
            Character.TexturesHans[7] = this.Content.Load<Texture2D>("new_party_hans");
            Character.TexturesHans_Portrait = this.Content.Load<Texture2D>("new_portrait_hans");

            Character.TexturesMonica[0] = this.Content.Load<Texture2D>("new_party_monica");
            Character.TexturesMonica[1] = this.Content.Load<Texture2D>("new_party_monica");
            Character.TexturesMonica[2] = this.Content.Load<Texture2D>("new_party_monica");
            Character.TexturesMonica[3] = this.Content.Load<Texture2D>("new_party_monica");
            Character.TexturesMonica[4] = this.Content.Load<Texture2D>("new_party_monica");
            Character.TexturesMonica[5] = this.Content.Load<Texture2D>("new_party_monica");
            Character.TexturesMonica[6] = this.Content.Load<Texture2D>("new_party_monica");
            Character.TexturesMonica[7] = this.Content.Load<Texture2D>("new_party_monica");
            Character.TexturesMonica_Portrait = this.Content.Load<Texture2D>("new_portrait_monica");

            Character.TexturesNinja[0] = this.Content.Load<Texture2D>("new_enemies_ninja");
            Character.TexturesNinja[1] = this.Content.Load<Texture2D>("new_enemies_ninja");
            Character.TexturesNinja[2] = this.Content.Load<Texture2D>("new_enemies_ninja");
            Character.TexturesNinja[3] = this.Content.Load<Texture2D>("new_enemies_ninja");
            Character.TexturesNinja[4] = this.Content.Load<Texture2D>("new_enemies_ninja");
            Character.TexturesNinja[5] = this.Content.Load<Texture2D>("new_enemies_ninja");
            Character.TexturesNinja[6] = this.Content.Load<Texture2D>("new_enemies_ninja");
            Character.TexturesNinja[7] = this.Content.Load<Texture2D>("new_enemies_ninja");

            Character.TexturesBanana[0] = this.Content.Load<Texture2D>("new_enemies_banana");
            Character.TexturesBanana[1] = this.Content.Load<Texture2D>("new_enemies_banana");
            Character.TexturesBanana[2] = this.Content.Load<Texture2D>("new_enemies_banana");
            Character.TexturesBanana[3] = this.Content.Load<Texture2D>("new_enemies_banana");
            Character.TexturesBanana[4] = this.Content.Load<Texture2D>("new_enemies_banana");
            Character.TexturesBanana[5] = this.Content.Load<Texture2D>("new_enemies_banana");
            Character.TexturesBanana[6] = this.Content.Load<Texture2D>("new_enemies_banana");
            Character.TexturesBanana[7] = this.Content.Load<Texture2D>("new_enemies_banana");

            Character.TexturesWitch[0] = this.Content.Load<Texture2D>("new_enemies_witch");
            Character.TexturesWitch[1] = this.Content.Load<Texture2D>("new_enemies_witch");
            Character.TexturesWitch[2] = this.Content.Load<Texture2D>("new_enemies_witch");
            Character.TexturesWitch[3] = this.Content.Load<Texture2D>("new_enemies_witch");
            Character.TexturesWitch[4] = this.Content.Load<Texture2D>("new_enemies_witch");
            Character.TexturesWitch[5] = this.Content.Load<Texture2D>("new_enemies_witch");
            Character.TexturesWitch[6] = this.Content.Load<Texture2D>("new_enemies_witch");
            Character.TexturesWitch[7] = this.Content.Load<Texture2D>("new_enemies_witch");

            Character.TexturesSamurai[0] = this.Content.Load<Texture2D>("new_enemies_samurai");
            Character.TexturesSamurai[1] = this.Content.Load<Texture2D>("new_enemies_samurai");
            Character.TexturesSamurai[2] = this.Content.Load<Texture2D>("new_enemies_samurai");
            Character.TexturesSamurai[3] = this.Content.Load<Texture2D>("new_enemies_samurai");
            Character.TexturesSamurai[4] = this.Content.Load<Texture2D>("new_enemies_samurai");
            Character.TexturesSamurai[5] = this.Content.Load<Texture2D>("new_enemies_samurai");
            Character.TexturesSamurai[6] = this.Content.Load<Texture2D>("new_enemies_samurai");
            Character.TexturesSamurai[7] = this.Content.Load<Texture2D>("new_enemies_samurai");

            Character.TexturesStanes[0] = this.Content.Load<Texture2D>("new_enemies_stanes");
            Character.TexturesStanes[1] = this.Content.Load<Texture2D>("new_enemies_stanes");
            Character.TexturesStanes[2] = this.Content.Load<Texture2D>("new_enemies_stanes");
            Character.TexturesStanes[3] = this.Content.Load<Texture2D>("new_enemies_stanes");
            Character.TexturesStanes[4] = this.Content.Load<Texture2D>("new_enemies_stanes");
            Character.TexturesStanes[5] = this.Content.Load<Texture2D>("new_enemies_stanes");
            Character.TexturesStanes[6] = this.Content.Load<Texture2D>("new_enemies_stanes");
            Character.TexturesStanes[7] = this.Content.Load<Texture2D>("new_enemies_stanes");

            Character.TexturesRussell[0] = this.Content.Load<Texture2D>("new_enemies_russell");
            Character.TexturesRussell[1] = this.Content.Load<Texture2D>("new_enemies_russell");
            Character.TexturesRussell[2] = this.Content.Load<Texture2D>("new_enemies_russell");
            Character.TexturesRussell[3] = this.Content.Load<Texture2D>("new_enemies_russell");
            Character.TexturesRussell[4] = this.Content.Load<Texture2D>("new_enemies_russell");
            Character.TexturesRussell[5] = this.Content.Load<Texture2D>("new_enemies_russell");
            Character.TexturesRussell[6] = this.Content.Load<Texture2D>("new_enemies_russell");
            Character.TexturesRussell[7] = this.Content.Load<Texture2D>("new_enemies_russell");

            TitleScreen.Texture = this.Content.Load<Texture2D>("new_titlescreen");

            portraitBox.Size = new Vector2(160, 160);
            largeCharacterStatsMainBox.Size = new Vector2(418, 414);
            largeCharacterStatsKillsDefeatsBox.Size = new Vector2(portraitBox.Size.X, 192);
            largeCharacterStatsGoldBox.Size = new Vector2(portraitBox.Size.X, largeCharacterStatsMainBox.Size.Y - portraitBox.Size.Y - largeCharacterStatsKillsDefeatsBox.Size.Y);
            // 2024.12.02 introduced constants
            portraitBox.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - largeCharacterStatsMainBox.Size.X - portraitBox.Size.X) / 2, (Game1.PREFERREDBACKBUFFERHEIGHT - largeCharacterStatsMainBox.Size.Y) / 2);
            largeCharacterStatsMainBox.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - largeCharacterStatsMainBox.Size.X - portraitBox.Size.X) / 2 + portraitBox.Size.X, (Game1.PREFERREDBACKBUFFERHEIGHT - largeCharacterStatsMainBox.Size.Y) / 2);
            largeCharacterStatsKillsDefeatsBox.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - largeCharacterStatsMainBox.Size.X - portraitBox.Size.X) / 2, (Game1.PREFERREDBACKBUFFERHEIGHT - largeCharacterStatsMainBox.Size.Y) / 2 + portraitBox.Size.Y);
            largeCharacterStatsGoldBox.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - largeCharacterStatsMainBox.Size.X - portraitBox.Size.X) / 2, (Game1.PREFERREDBACKBUFFERHEIGHT - largeCharacterStatsMainBox.Size.Y) / 2 + portraitBox.Size.Y + largeCharacterStatsKillsDefeatsBox.Size.Y);

            whoseStatsToBeDisplayed = new CharacterPointer();

            for (int i = 0; i < Party.MAXPARTYMEMBERS; i++)
                targetsAttackedOrHealed[i] = new CharacterPointer();

            keyHoldCounter = 0;
            lockDraw = false;
            lockInput = false;

            positionInTextMessage = new PositionInTextMessage();

            // items
            nothing = new Item("", "Nothing");
            medicalHerb = new Item("Medical", " Herb");
            
            // temporarily speed is set to 1
            speed = 1;

            SetUpInitialParty();

            GameMode = GameModes.TitleScreenMode;

            base.Initialize();
        }

        // 2015.06.27 Moved this from Initialize to a separate method so that it can be called from other places as well
        private void SetUpInitialParty()
        {
            // characters
            hero = new Character("JACOB", 1, 1, Character.PromotedStatuses.No, 0, 0, 0, new Item[] { new Item("Wooden", " Sword"), null, null, null }, new bool[] { true, false, false, false });
            healer = new Character("CARYN", 1, 1, Character.PromotedStatuses.No, 0, 0, 0, new Item[] { new Item("Wooden", " Staff"), null, null, null }, new bool[] { true, false, false, false });
            warrior = new Character("TASSI", 1, 1, Character.PromotedStatuses.No, 0, 0, 0, new Item[] { new Item("Wooden", " Sword"), null, null, null }, new bool[] { true, false, false, false });
            archer = new Character("EVA", 1, 1, Character.PromotedStatuses.No, 0, 0, 0, new Item[] { new Item("Wooden", " Arrow"), null, null, null }, new bool[] { true, false, false, false });
            knight = new Character("HANS", 1, 1, Character.PromotedStatuses.No, 0, 0, 0, new Item[] { new Item("Wooden", " Sword"), null, null, null }, new bool[] { true, false, false, false });
            mage = new Character("MONICA", 1, 1, Character.PromotedStatuses.No, 0, 0, 0, new Item[] { new Item("Wooden", " Staff"), null, null, null }, new bool[] { true, false, false, false });

            // temporarily the party consists only of jacob, caryn and tassi
            party = new Party(3, CharacterPointer.Sides.Player);
            party.Members[0] = hero;
            party.Members[1] = healer;
            party.Members[2] = warrior;

            /*
            party.Members[3] = kazin;
            party.Members[4] = slade;
            party.Members[5] = kiwi;
            party.Members[6] = gerhalt;
            party.Members[7] = luke;
            party.Members[8] = rick;
            party.Members[9] = elric;
            party.Members[10] = karna;
            party.Members[11] = janet;
            */
        }

        private void ResetMapPositionPlayerMoveMode()
        {
            for (int i = 0; i < party.NumPartyMembers; i++)
                mapPositionPlayerMoveMode[i] = map.CenterMapPosition((int)party.Members[i].Position);
        }

        private void ResetMapPositionEnemyMoveMode()
        {
            for (int i = 0; i < enemies.NumPartyMembers; i++)
                mapPositionEnemyMoveMode[i] = map.CenterMapPosition((int) enemies.Members[i].Position);
        }

        private CharacterPointer CalculateCharacterPointersForThisBattle(Party[] parties)
        {
            PartySet[,] parties_set = new PartySet[2, Party.MAXPARTYMEMBERS];
            int[] parties_numSet = new int[2];
            CharacterPointer highest = null;
            CharacterPointer previous = null;
            CharacterPointer first = null;
            int i;
            int j;

            parties_numSet[0] = 0;
            parties_numSet[1] = 0;

            for (i = 0; i < Party.MAXPARTYMEMBERS; i++)
            {
                parties_set[0,i] = PartySet.NotSet;
                parties_set[1,i] = PartySet.NotSet;
            }

            // search for the character with the highest agility first, then descending
            while (parties_numSet[0] < parties[0].NumPartyMembers || parties_numSet[1] < parties[1].NumPartyMembers)
            {
                highest = null;
                for (j = 0; j <= 1; j++)
                {
                    for (i = 0; i < parties[j].NumPartyMembers; i++)
                    {
                        if (parties_set[j, i] == PartySet.NotSet)
                        {
                            if (!parties[j].Members[i].Alive)
                            {
                                parties_set[j, i] = PartySet.SetForGood;
                                parties_numSet[j]++;
                            }
                            else if (highest == null)
                                highest = new CharacterPointer(parties[j].Side, i, parties[j].Members[i].Agility);
                            else if ((parties[j].Members[i].Agility == highest.Agility && random.Next(100) % 2 == 0)
                               || parties[j].Members[i].Agility > highest.Agility)
                            {
                                highest.BelongsToSide = parties[j].Side;
                                highest.WhichOne = i;
                                highest.Agility = parties[j].Members[i].Agility;
                            }
                        }
                        else if (parties_set[j, i] == PartySet.SetOnce)
                        {
                            if (highest == null)
                                highest = new CharacterPointer(parties[j].Side, i, parties[j].Members[i].Agility / 2);
                            else if (parties[j].Members[i].Agility / 2 > highest.Agility)
                            {
                                highest.BelongsToSide = parties[j].Side;
                                highest.WhichOne = i;
                                highest.Agility = parties[j].Members[i].Agility / 2;
                            }
                        }
                    }
                }

                for (j = 0; j <= 1; j++)
                {
                    if (highest.BelongsToSide == parties[j].Side)
                    {
                        if (parties[j].Members[highest.WhichOne].Boss)
                        {
                            if (parties_set[j, highest.WhichOne] == PartySet.SetOnce)
                            {
                                parties_set[j, highest.WhichOne] = PartySet.SetForGood;
                                parties_numSet[j]++;
                            }
                            else
                                parties_set[j, highest.WhichOne] = PartySet.SetOnce;
                        }
                        else
                        {
                            parties_set[j, highest.WhichOne] = PartySet.SetForGood;
                            parties_numSet[j]++;
                        }
                    }
                }

                if (first == null)
                    first = highest;

                if (previous != null)
                    previous.Next = highest;

                previous = highest;
            }

            //if (highest != null)
            //    highest.Next = first;

            return first;
        }

        public void NextCharacter()
        {
            if (characterPointer == null)
            {
                turn++;
                characterPointer = CalculateCharacterPointersForThisBattle(new Party[] { party, enemies });
            }

            party.Members[party.MemberOnTurn].Visible = true;
            enemies.Members[enemies.MemberOnTurn].Visible = true;

            if (characterPointer.BelongsToSide == CharacterPointer.Sides.Player)
            {
                party.MemberOnTurn = characterPointer.WhichOne;
                if (party.Members[party.MemberOnTurn].Alive)
                {
                    backUpPosition = party.Members[party.MemberOnTurn].Position;
                    characterStatsBox.Position = new Vector2(0, (Game1.PREFERREDBACKBUFFERHEIGHT - 540) / 2);
                    EnterPlayerMoveTransitionMode();
                }
                else
                {
                    characterPointer = characterPointer.Next;
                    NextCharacter();
                }   
            }
            else
            {
                if (DEBUG_SKIPENEMY)
                {
                    characterPointer = characterPointer.Next;
                    NextCharacter();
                }
                else
                {
                    enemies.MemberOnTurn = characterPointer.WhichOne;
                    if (enemies.Members[enemies.MemberOnTurn].Alive)
                    {
                        backUpPosition = enemies.Members[enemies.MemberOnTurn].Position;
                        characterStatsBox.Position = new Vector2(0, (Game1.PREFERREDBACKBUFFERHEIGHT - 540) / 2);
                        EnterEnemyMoveTransitionMode();
                    }
                    else
                    {
                        characterPointer = characterPointer.Next;
                        NextCharacter();
                    }
                }
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
            spriteBatch = new SpriteBatch(this.graphics.GraphicsDevice);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (gameTimeAtLastBlink == 0)
                gameTimeAtLastBlink = gameTime.TotalGameTime.TotalMilliseconds;

            // TODO: Add your update logic here
            UpdateVariables();          
            UpdateInput();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (!lockDraw)
            {
                graphics.GraphicsDevice.Clear(Color.Black);

                // TODO: Add your drawing code here
                switch (GameMode)
                {
                    case GameModes.SelectionBarMode:
                        Draw_SelectionBarMode(gameTime);
                        break;

                    case GameModes.PlayerMoveMode:
                        Draw_PlayerMoveMode(gameTime);
                        break;

                    case GameModes.SelectionBarTransitionMode:
                        Draw_SelectionBarTransitionMode(gameTime);
                        break;

                    case GameModes.PlayerMoveTransitionMode:
                        Draw_PlayerMoveTransitionMode(gameTime);
                        break;

                    case GameModes.PlayerMovingMode:
                        Draw_PlayerMovingMode(gameTime);
                        break;

                    case GameModes.BattleMenuMode:
                        Draw_BattleMenuMode(gameTime);
                        break;

                    case GameModes.PlayerMovingInSelectionBarTransitionMode:
                        Draw_PlayerMovingInSelectionBarTransitionMode(gameTime);
                        break;

                    case GameModes.TransitionInPlayerMoveTransitionMode:
                        Draw_TransitionInPlayerMoveTransitionMode(gameTime);
                        break;

                    case GameModes.TransitionInSelectionBarMode:
                        Draw_TransitionInSelectionBarMode(gameTime);
                        break;

                    case GameModes.LargeCharacterStatsMode:
                        Draw_LargeCharacterStatsMode(gameTime);
                        break;

                    case GameModes.SmallCharacterStatsMode:
                        Draw_SmallCharacterStatsMode(gameTime);
                        break;

                    case GameModes.AttackMenuMode:
                        Draw_AttackMenuMode(gameTime);
                        break;

                    case GameModes.TransitionInAttackMenuMode:
                        Draw_TransitionInAttackMenuMode(gameTime);
                        break;

                    case GameModes.SelectionBarTransitionOutAttackMenuMode:
                        Draw_SelectionBarTransitionOutAttackMenuMode(gameTime);
                        break;

                    case GameModes.GeneralMenuMode:
                        Draw_GeneralMenuMode(gameTime);
                        break;

                    case GameModes.ItemMenuMode:
                        Draw_ItemMenuMode(gameTime);
                        break;

                    case GameModes.GeneralMenuMoveOutMode:
                        Draw_GeneralMenuMoveOutMode(gameTime);
                        break;

                    case GameModes.GeneralMenuMoveInMode:
                        Draw_GeneralMenuMoveInMode(gameTime);
                        break;

                    case GameModes.BattleMenuMoveOutMode:
                        Draw_BattleMenuMoveOutMode(gameTime);
                        break;

                    case GameModes.BattleMenuMoveInMode:
                        Draw_BattleMenuMoveInMode(gameTime);
                        break;

                    case GameModes.ItemMenuMoveOutMode:
                        Draw_ItemMenuMoveOutMode(gameTime);
                        break;

                    case GameModes.ItemMenuMoveInMode:
                        Draw_ItemMenuMoveInMode(gameTime);
                        break;

                    case GameModes.ItemMagicSelectionMenuMode:
                        Draw_ItemMagicSelectionMenuMode(gameTime);
                        break;

                    case GameModes.ItemMagicSelectionMenuMoveInMode:
                        Draw_ItemMagicSelectionMenuMoveInMode(gameTime);
                        break;

                    case GameModes.ItemMagicSelectionMenuMoveOutMode:
                        Draw_ItemMagicSelectionMenuMoveOutMode(gameTime);
                        break;

                    case GameModes.DisplayTextMessageMode:
                        Draw_DisplayTextMessageMode(gameTime);
                        break;

                    case GameModes.DisplayTextMessageMoveOutMode:
                        Draw_DisplayTextMessageMoveOutMode(gameTime);
                        break;

                    case GameModes.DropItemMode:
                        Draw_DropItemMode(gameTime);
                        break;

                    case GameModes.GiveItemMode:
                        Draw_GiveItemMode(gameTime);
                        break;

                    case GameModes.SelectionBarTransitionInGiveItemMode:
                        Draw_SelectionBarTransitionInGiveItemMode(gameTime);
                        break;

                    case GameModes.SelectionBarTransitionOutGiveItemMode:
                        Draw_SelectionBarTransitionOutGiveItemMode(gameTime);
                        break;

                    case GameModes.SwapItemMode:
                        Draw_SwapItemMode(gameTime);
                        break;

                    case GameModes.SwapItemMoveInMode:
                        Draw_SwapItemMoveInMode(gameTime);
                        break;

                    case GameModes.SwapItemMoveOutMode:
                        Draw_SwapItemMoveOutMode(gameTime);
                        break;

                    case GameModes.EquipWeaponMode:
                        Draw_EquipWeaponMode(gameTime);
                        break;

                    case GameModes.EquipWeaponMoveInMode:
                        Draw_EquipWeaponMoveInMode(gameTime);
                        break;

                    case GameModes.EquipWeaponMoveOutMode:
                        Draw_EquipWeaponMoveOutMode(gameTime);
                        break;

                    case GameModes.EquipRingMode:
                        Draw_EquipRingMode(gameTime);
                        break;

                    case GameModes.EquipRingMoveInMode:
                        Draw_EquipRingMoveInMode(gameTime);
                        break;

                    case GameModes.EquipRingMoveOutMode:
                        Draw_EquipRingMoveOutMode(gameTime);
                        break;

                    case GameModes.PlayerBattleMode:
                        Draw_PlayerBattleMode(gameTime);
                        break;

                    case GameModes.DisplayDefeatedCharactersMode:
                        Draw_DisplayDefeatedCharactersMode(gameTime);
                        break;

                    case GameModes.NextCharacterAfterSleepMode:
                        Draw_NextCharacterAfterSleepMode(gameTime);
                        break;

                    case GameModes.PlayerBattleDisplayTextMessageMode:
                        Draw_PlayerBattleDisplayTextMessageMode(gameTime);
                        break;

                    case GameModes.BattleDisplayTextMessageMoveOutMode:
                        Draw_BattleDisplayTextMessageMoveOutMode(gameTime);
                        break;

                    case GameModes.MagicLevelSelectionMode:
                        Draw_MagicLevelSelectionMode(gameTime);
                        break;

                    case GameModes.EnemyMoveTransitionMode:
                        Draw_EnemyMoveTransitionMode(gameTime);
                        break;

                    case GameModes.TransitionInEnemyMoveTransitionMode:
                        Draw_TransitionInEnemyMoveTransitionMode(gameTime);
                        break;

                    case GameModes.EnemyMoveMode:
                        Draw_EnemyMoveMode(gameTime);
                        break;

                    case GameModes.EnemyMovingMode:
                        Draw_EnemyMovingMode(gameTime);
                        break;

                    case GameModes.EnemyBattleMode:
                        Draw_EnemyBattleMode(gameTime);
                        break;

                    case GameModes.EnemyBattleDisplayTextMessageMode:
                        Draw_EnemyBattleDisplayTextMessageMode(gameTime);
                        break;

                    case GameModes.EnemyAttackMode:
                        Draw_EnemyAttackMode(gameTime);
                        break;

                    case GameModes.TransitionInEnemyAttackMode:
                        Draw_TransitionInEnemyAttackMode(gameTime);
                        break;

                    case GameModes.HealMenuMode:
                        Draw_HealMenuMode(gameTime);
                        break;

                    case GameModes.TransitionInHealMenuMode:
                        Draw_TransitionInHealMenuMode(gameTime);
                        break;

                    case GameModes.EndTurnOfCharacterAndNextCharacterMode:
                        Draw_EndTurnOfCharacterAndNextCharacterMode(gameTime);
                        break;

                    case GameModes.PlayerAutoMoveMode:
                        Draw_PlayerAutoMoveMode(gameTime);
                        break;

                    case GameModes.PlayerAutoAttackMode:
                        Draw_PlayerAutoAttackMode(gameTime);
                        break;

                    case GameModes.BattleFieldFadeOutMode:
                        Draw_BattleFieldFadeOutMode(gameTime);
                        break;

                    case GameModes.BattleFieldFadeInMode:
                        Draw_BattleFieldFadeInMode(gameTime);
                        break;

                    case GameModes.MemberMenuMode:
                        Draw_MemberMenuMode(gameTime);
                        break;

                    case GameModes.TitleScreenMode:
                        Draw_TitleScreenMode(gameTime);
                        break;

                    case GameModes.StoryMode:
                        Draw_StoryMode(gameTime);
                        break;
                }

                base.Draw(gameTime);
            }
        }

        private void Draw_SelectionBarMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, map.Position, false, false);
            Blink(gameTime);
            selectionBar.Draw(spriteBatch, oldSelectionBarPosition);
            party.DrawAt(spriteBatch, map, map.Position, false, map.Offset);
            enemies.DrawAt(spriteBatch, map, map.Position, false, map.Offset);
            spriteBatch.End();
        }

        private void Draw_PlayerMoveMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch);
            Blink(gameTime);
            enemies.Draw(spriteBatch, map);
            party.Draw(spriteBatch, map);
            party.Members[party.MemberOnTurn].Draw(spriteBatch, map.CalcPosition(party.Members[party.MemberOnTurn].Position));
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            spriteBatch.End();
        }

        private void Draw_SelectionBarTransitionMode(GameTime gameTime)
        {
            if (party.Members[party.MemberOnTurn].Position == backUpPosition)
            {
                spriteBatch.Begin();
                map.Draw(spriteBatch, map.Position, false, false);
                Blink(gameTime);
                party.Draw(spriteBatch, map);
                enemies.Draw(spriteBatch, map);
                selectionBar.Draw(spriteBatch, oldSelectionBarPosition);
                spriteBatch.End();
            }
            else
                Draw_PlayerMovingInSelectionBarTransitionMode(gameTime);
        }

        private void Draw_PlayerMoveTransitionMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, oldMapPosition, false, false);
            Blink(gameTime);
            party.Draw(spriteBatch, map, oldMapPosition);
            enemies.Draw(spriteBatch, map, oldMapPosition);
            selectionBar.Draw(spriteBatch, oldSelectionBarPosition);
            spriteBatch.End();
        }

        private void Draw_PlayerMovingMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch);
            Blink(gameTime);
            party.DrawAt(spriteBatch, map, map.Position, true, map.Offset);
            enemies.DrawAt(spriteBatch, map, map.Position, false, map.Offset);
            party.Members[party.MemberOnTurn].DrawAt(spriteBatch, delayPosition + new Vector2(Character.OFFSETX, Character.OFFSETY));
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            spriteBatch.End();
        }

        private void Draw_BattleMenuMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            battleMenu.Draw(spriteBatch);
            captionBox.Draw(spriteBatch);
            smallFont.Position = captionBox.Position + new Vector2(20, 16);
            switch (battleMenu.CurrentState)
            {
                case BattleMenu.States.AttackSelected1:
                case BattleMenu.States.AttackSelected2:
                    smallFont.Print(spriteBatch, "ATTACK");
                    break;

                case BattleMenu.States.ItemSelected1:
                case BattleMenu.States.ItemSelected2:
                    smallFont.Print(spriteBatch, "ITEM");
                    break;

                case BattleMenu.States.MagicSelected1:
                case BattleMenu.States.MagicSelected2:
                    smallFont.Print(spriteBatch, "MAGIC");
                    break;

                case BattleMenu.States.StaySelected1:
                case BattleMenu.States.StaySelected2:
                    smallFont.Print(spriteBatch, "STAY");
                    break;
            }
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            spriteBatch.End();
        }

        private void Draw_PlayerMovingInSelectionBarTransitionMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, map.Position, false, false);
            Blink(gameTime);
            party.Draw(spriteBatch, map, map.Position, true);
            enemies.Draw(spriteBatch, map);
            party.Members[party.MemberOnTurn].DrawAt(spriteBatch, delayPosition + new Vector2(Character.OFFSETX, Character.OFFSETY));
            spriteBatch.End();
        }

        private void Draw_TransitionInPlayerMoveTransitionMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, oldMapPosition, false, false);
            Blink(gameTime);
            party.DrawAt(spriteBatch, map, oldMapPosition, false, map.Offset);
            enemies.DrawAt(spriteBatch, map, oldMapPosition, false, map.Offset); 
            selectionBar.DrawAt(spriteBatch, tempSelectionBarPosition);
            spriteBatch.End();
        }

        private void Draw_TransitionInSelectionBarMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, map.Position, false, false);
            Blink(gameTime);
            enemies.DrawAt(spriteBatch, map, map.Position, false, map.Offset);
            party.DrawAt(spriteBatch, map, map.Position, false, map.Offset);
            selectionBar.DrawAt(spriteBatch, tempSelectionBarPosition);
            spriteBatch.End();
        }

        private void Draw_LargeCharacterStatsMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, map.Position, false, false);
            Blink(gameTime);
            enemies.Draw(spriteBatch, map);
            party.Draw(spriteBatch, map);
            largeCharacterStatsMainBox.Draw(spriteBatch);
            largeCharacterStatsKillsDefeatsBox.Draw(spriteBatch);
            smallFont.Position = largeCharacterStatsKillsDefeatsBox.Position + new Vector2(20, 96);
            smallFont.Println(spriteBatch, "KILLS");
            // *** icons for poisoned, silenced, stunned, asleep, cursed still have to be added
            if (whoseStatsToBeDisplayed.BelongsToSide == CharacterPointer.Sides.Player)
            {
                portraitBox.Draw(spriteBatch);
                largeCharacterStatsGoldBox.Draw(spriteBatch);
                party.Members[whoseStatsToBeDisplayed.WhichOne].DrawPortraitAt(spriteBatch, portraitBox.Position + new Vector2(20, 15));
                party.Members[whoseStatsToBeDisplayed.WhichOne].DrawAt(spriteBatch, largeCharacterStatsKillsDefeatsBox.Position + new Vector2(48, 28), true);
                smallFont.Println(spriteBatch, RightAlign(party.Members[whoseStatsToBeDisplayed.WhichOne].NumKills.ToString(), 6));
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, "DEFEAT");
                smallFont.Println(spriteBatch, RightAlign(party.Members[whoseStatsToBeDisplayed.WhichOne].NumDefeats.ToString(), 6));
                smallFont.Position = largeCharacterStatsGoldBox.Position + new Vector2(20, 16);
                smallFont.Println(spriteBatch, "GOLD");
                smallFont.Println(spriteBatch, RightAlign(gold.ToString(), 6));
                smallFont.Position = largeCharacterStatsMainBox.Position + new Vector2(20, 16);
                smallFont.Println(spriteBatch, party.Members[whoseStatsToBeDisplayed.WhichOne].CharClass + " " + party.Members[whoseStatsToBeDisplayed.WhichOne].Name);
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, " LV " + RightAlign(itoa(party.Members[whoseStatsToBeDisplayed.WhichOne].LevelToDisplay), 5));
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, " HP " + RightAlign(itoa(party.Members[whoseStatsToBeDisplayed.WhichOne].HitPoints), 2) + "/" + RightAlign(itoa(party.Members[whoseStatsToBeDisplayed.WhichOne].MaxHitPoints), 2));
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, " MP " + RightAlign(itoa(party.Members[whoseStatsToBeDisplayed.WhichOne].MagicPoints), 2) + "/" + RightAlign(itoa(party.Members[whoseStatsToBeDisplayed.WhichOne].MaxMagicPoints), 2));
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, " EXP " + RightAlign(itoa(party.Members[whoseStatsToBeDisplayed.WhichOne].ExperiencePoints), 4));
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, "MAGIC     ITEM");
                if (party.Members[whoseStatsToBeDisplayed.WhichOne].Items == null)
                {
                    smallFont.Println(spriteBatch, "");
                    smallFont.Print(spriteBatch, "           Nothing", new Color(231, 130, 66));
                }
                else
                    for (int i = 0; i < Character.MAXNUMBER_ITEMS; i++)
                        if (party.Members[whoseStatsToBeDisplayed.WhichOne].Items[i] != null)
                        {
                            party.Members[whoseStatsToBeDisplayed.WhichOne].Items[i].DrawAt(spriteBatch, largeCharacterStatsMainBox.Position + new Vector2(206, 207 + i * 48));
                            smallFont.Position = largeCharacterStatsMainBox.Position + new Vector2(248, 207 + i * 48);
                            smallFont.Println(spriteBatch, party.Members[whoseStatsToBeDisplayed.WhichOne].Items[i].Name1);
                            smallFont.Println(spriteBatch, party.Members[whoseStatsToBeDisplayed.WhichOne].Items[i].Name2);
                            if (party.Members[whoseStatsToBeDisplayed.WhichOne].ItemEquipped != null && party.Members[whoseStatsToBeDisplayed.WhichOne].ItemEquipped[i])
                                smallFont.Print(spriteBatch, "Equipped", new Color(231, 130, 66));
                        }
                if (party.Members[whoseStatsToBeDisplayed.WhichOne].MagicSpells == null)
                {
                    smallFont.Position = largeCharacterStatsMainBox.Position + new Vector2(20 + smallFont.Size.X, 16 + smallFont.Size.Y * 13);
                    smallFont.Print(spriteBatch, "Nothing", new Color(231, 130, 66));
                }
                else
                    for (int i = 0; i < Character.MAXNUMBER_SPELLS; i++)
                        if (party.Members[whoseStatsToBeDisplayed.WhichOne].MagicSpells[i] != null)
                        {
                            party.Members[whoseStatsToBeDisplayed.WhichOne].MagicSpells[i].DrawAt(spriteBatch, largeCharacterStatsMainBox.Position + new Vector2(18, 16 + smallFont.Size.Y * 12 + i * 48));
                            smallFont.Position = largeCharacterStatsMainBox.Position + new Vector2(60, 16 + smallFont.Size.Y * 12 + i * 48);
                            smallFont.Println(spriteBatch, party.Members[whoseStatsToBeDisplayed.WhichOne].MagicSpells[i].Name);
                            for (int j = 0; j < party.Members[whoseStatsToBeDisplayed.WhichOne].MagicSpells[i].Level; j++)
                                spriteBatch.Draw(spellLevelSelectedTexture, largeCharacterStatsMainBox.Position + new Vector2(60 + j * 30, 16 + smallFont.Size.Y * 14 + i * 48), Color.White);
                        }
                smallFont.Position = largeCharacterStatsMainBox.Position + new Vector2(20 + smallFont.Size.X * 12, 16 + smallFont.Size.Y * 2);
                smallFont.Println(spriteBatch, "ATT " + RightAlign(itoa(party.Members[whoseStatsToBeDisplayed.WhichOne].AttackPoints), 3));
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, "DEF " + RightAlign(itoa(party.Members[whoseStatsToBeDisplayed.WhichOne].DefensePoints), 3));
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, "AGI " + RightAlign(itoa(party.Members[whoseStatsToBeDisplayed.WhichOne].Agility), 3));
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, "MOV " + RightAlign(itoa(party.Members[whoseStatsToBeDisplayed.WhichOne].MovePoints), 3));
            }
            else
            {
                enemies.Members[whoseStatsToBeDisplayed.WhichOne].DrawAt(spriteBatch, largeCharacterStatsKillsDefeatsBox.Position + new Vector2(48, 28), true);
                smallFont.Println(spriteBatch, "     ?");
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, "DEFEAT");
                smallFont.Println(spriteBatch, "     ?");
                smallFont.Position = largeCharacterStatsMainBox.Position + new Vector2(20, 16);
                smallFont.Println(spriteBatch, enemies.Members[whoseStatsToBeDisplayed.WhichOne].CharClass + " " + enemies.Members[whoseStatsToBeDisplayed.WhichOne].Name);
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, " LV   N/A");
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, " HP " + RightAlign(itoa(enemies.Members[whoseStatsToBeDisplayed.WhichOne].HitPoints) + "/" + itoa(enemies.Members[whoseStatsToBeDisplayed.WhichOne].MaxHitPoints), 5));
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, " MP " + RightAlign(itoa(enemies.Members[whoseStatsToBeDisplayed.WhichOne].MagicPoints) + "/" + itoa(enemies.Members[whoseStatsToBeDisplayed.WhichOne].MaxMagicPoints), 5));
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, " EXP  N/A");
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, "MAGIC     ITEM");
                if (enemies.Members[whoseStatsToBeDisplayed.WhichOne].Items == null)
                {
                    smallFont.Println(spriteBatch, "");
                    smallFont.Print(spriteBatch, "           Nothing", new Color(231, 130, 66));
                }
                else
                    for (int i = 0; i < 4; i++)
                        if (enemies.Members[whoseStatsToBeDisplayed.WhichOne].Items[i] != null)
                        {
                            enemies.Members[whoseStatsToBeDisplayed.WhichOne].Items[i].DrawAt(spriteBatch, largeCharacterStatsMainBox.Position + new Vector2(206, 207 + i * 48));
                            smallFont.Position = largeCharacterStatsMainBox.Position + new Vector2(248, 207 + i * 48);
                            smallFont.Println(spriteBatch, enemies.Members[whoseStatsToBeDisplayed.WhichOne].Items[i].Name1);
                            smallFont.Println(spriteBatch, enemies.Members[whoseStatsToBeDisplayed.WhichOne].Items[i].Name2);
                            if (enemies.Members[whoseStatsToBeDisplayed.WhichOne].ItemEquipped[i])
                                smallFont.Print(spriteBatch, "Equipped", new Color(231, 130, 66));
                        }
                if (enemies.Members[whoseStatsToBeDisplayed.WhichOne].MagicSpells == null)
                {
                    smallFont.Position = largeCharacterStatsMainBox.Position + new Vector2(20 + smallFont.Size.X, 16 + smallFont.Size.Y * 13);
                    smallFont.Print(spriteBatch, "Nothing", new Color(231, 130, 66));
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                        if (enemies.Members[whoseStatsToBeDisplayed.WhichOne].MagicSpells[i] != null)
                        {
                            enemies.Members[whoseStatsToBeDisplayed.WhichOne].MagicSpells[i].DrawAt(spriteBatch, largeCharacterStatsMainBox.Position + new Vector2(18, 16 + smallFont.Size.Y * 12 + i * 48));
                            smallFont.Position = largeCharacterStatsMainBox.Position + new Vector2(60, 16 + smallFont.Size.Y * 12 + i * 48);
                            smallFont.Println(spriteBatch, enemies.Members[whoseStatsToBeDisplayed.WhichOne].MagicSpells[i].Name);
                            for (int j = 0; j < enemies.Members[whoseStatsToBeDisplayed.WhichOne].MagicSpells[i].Level; j++)
                                spriteBatch.Draw(spellLevelSelectedTexture, largeCharacterStatsMainBox.Position + new Vector2(60 + j * 30, 16 + smallFont.Size.Y * 14 + i * 48), Color.White);
                        }
                }
                smallFont.Position = largeCharacterStatsMainBox.Position + new Vector2(20 + smallFont.Size.X * 12, 16 + smallFont.Size.Y * 2);
                smallFont.Println(spriteBatch, "ATT " + RightAlign(itoa(enemies.Members[whoseStatsToBeDisplayed.WhichOne].AttackPoints), 3));
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, "DEF " + RightAlign(itoa(enemies.Members[whoseStatsToBeDisplayed.WhichOne].DefensePoints), 3));
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, "AGI " + RightAlign(itoa(enemies.Members[whoseStatsToBeDisplayed.WhichOne].Agility), 3));
                smallFont.Println(spriteBatch, "");
                smallFont.Println(spriteBatch, "MOV " + RightAlign(itoa(enemies.Members[whoseStatsToBeDisplayed.WhichOne].MovePoints), 3));
            }
            spriteBatch.End();
        }

        private void Draw_SmallCharacterStatsMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, map.Position, false, false);
            Blink(gameTime);
            enemies.Draw(spriteBatch, map);
            party.Draw(spriteBatch, map);
            if (whoseStatsToBeDisplayed.BelongsToSide == CharacterPointer.Sides.Player)
            {
                Draw_LandEffectBox(party.Members[whoseStatsToBeDisplayed.WhichOne].Position);
                Draw_CharacterStatsBox(party.Members[whoseStatsToBeDisplayed.WhichOne], CharacterPointer.Sides.Player);
            }
            else
            {
                Draw_LandEffectBox(enemies.Members[whoseStatsToBeDisplayed.WhichOne].Position);
                Draw_CharacterStatsBox(enemies.Members[whoseStatsToBeDisplayed.WhichOne], CharacterPointer.Sides.CPU_Opponents);
            }
            spriteBatch.End();
        }

        private void Draw_AttackMenuMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, oldMapPosition, true);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            switch (battleMenu.CurrentState)
            {
                case BattleMenu.States.MagicSelected1:
                case BattleMenu.States.MagicSelected2:
                    switch (party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].Area[selectedMagicLevel - 1])
                    {
                        case 1:
                            selectionBar.Draw(spriteBatch, oldSelectionBarPosition);
                            break;

                        case 2:
                            selectionBar2.DrawAt(spriteBatch, new Vector2((oldSelectionBarPosition.X - 1) * Map.TILESIZEX, (oldSelectionBarPosition.Y - 1) * Map.TILESIZEY) + new Vector2(SelectionBar.OFFSETX, SelectionBar.OFFSETY));
                            break;

                        case 3:
                            selectionBar3.DrawAt(spriteBatch, new Vector2((oldSelectionBarPosition.X - 2) * Map.TILESIZEX, (oldSelectionBarPosition.Y - 2) * Map.TILESIZEY) + new Vector2(SelectionBar.OFFSETX, SelectionBar.OFFSETY));
                            break;
                    }
                    break;

                case BattleMenu.States.ItemSelected1:
                case BattleMenu.States.ItemSelected2:
                    switch (party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Area[party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1])
                    {
                        case 1:
                            selectionBar.Draw(spriteBatch, oldSelectionBarPosition);
                            break;

                        case 2:
                            selectionBar2.DrawAt(spriteBatch, new Vector2((oldSelectionBarPosition.X - 1) * Map.TILESIZEX, (oldSelectionBarPosition.Y - 1) * Map.TILESIZEY) + new Vector2(SelectionBar.OFFSETX, SelectionBar.OFFSETY));
                            break;

                        case 3:
                            selectionBar3.DrawAt(spriteBatch, new Vector2((oldSelectionBarPosition.X - 2) * Map.TILESIZEX, (oldSelectionBarPosition.Y - 2) * Map.TILESIZEY) + new Vector2(SelectionBar.OFFSETX, SelectionBar.OFFSETY));
                            break;
                    }
                    break;

                default:
                    selectionBar.Draw(spriteBatch, oldSelectionBarPosition);
                    break;
            }
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            Draw_TargetCharacterStatsBox();
            spriteBatch.End();
        }

        private void Draw_TransitionInAttackMenuMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, true);
            Blink(gameTime);
            party.DrawAt(spriteBatch, map, map.Position, false, map.Offset);
            enemies.DrawAt(spriteBatch, map, map.Position, false, map.Offset);
            switch (battleMenu.CurrentState)
            {
                case BattleMenu.States.MagicSelected1:
                case BattleMenu.States.MagicSelected2:
                    switch (party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].Area[selectedMagicLevel - 1])
                    {
                        case 1:
                            selectionBar.DrawAt(spriteBatch, tempSelectionBarPosition);
                            break;

                        case 2:
                            selectionBar2.DrawAt(spriteBatch, tempSelectionBarPosition - new Vector2(Map.TILESIZEX, Map.TILESIZEY));
                            break;

                        case 3:
                            selectionBar3.DrawAt(spriteBatch, tempSelectionBarPosition - new Vector2(Map.TILESIZEX * 2, Map.TILESIZEY * 2));
                            break;
                    }
                    break;

                case BattleMenu.States.ItemSelected1:
                case BattleMenu.States.ItemSelected2:
                    switch (party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Area[party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1])
                    {
                        case 1:
                            selectionBar.DrawAt(spriteBatch, tempSelectionBarPosition);
                            break;

                        case 2:
                            selectionBar2.DrawAt(spriteBatch, tempSelectionBarPosition - new Vector2(Map.TILESIZEX, Map.TILESIZEY));
                            break;

                        case 3:
                            selectionBar3.DrawAt(spriteBatch, tempSelectionBarPosition - new Vector2(Map.TILESIZEX * 2, Map.TILESIZEY * 2));
                            break;
                    }
                    break;

                default:
                    selectionBar.DrawAt(spriteBatch, tempSelectionBarPosition);
                    break;
            }
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            Draw_TargetCharacterStatsBox();
            spriteBatch.End();
        }

        private void Draw_SelectionBarTransitionOutAttackMenuMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, true);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            selectionBar.DrawAt(spriteBatch, tempSelectionBarPosition);
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            spriteBatch.End();
        }

        private void Draw_GeneralMenuMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, map.Position, false, false);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            generalMenu.Draw(spriteBatch);
            captionBox.Draw(spriteBatch);
            smallFont.Position = captionBox.Position + new Vector2(20, 16);
            switch (generalMenu.CurrentState)
            {
                case GeneralMenu.States.MapSelected1:
                case GeneralMenu.States.MapSelected2:
                    smallFont.Print(spriteBatch, "MAP");
                    break;

                case GeneralMenu.States.MemberSelected1:
                case GeneralMenu.States.MemberSelected2:
                    smallFont.Print(spriteBatch, "MEMBER");
                    break;

                case GeneralMenu.States.QuitSelected1:
                case GeneralMenu.States.QuitSelected2:
                    smallFont.Print(spriteBatch, "QUIT");
                    break;

                case GeneralMenu.States.SpeedSelected1:
                case GeneralMenu.States.SpeedSelected2:
                    smallFont.Print(spriteBatch, "SPEED");
                    break;
            }
            spriteBatch.End();
        }

        private void Draw_ItemMenuMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            itemMenu.Draw(spriteBatch);
            captionBox.Draw(spriteBatch);
            smallFont.Position = captionBox.Position + new Vector2(20, 16);
            switch (itemMenu.CurrentState)
            {
                case ItemMenu.States.DropSelected1:
                case ItemMenu.States.DropSelected2:
                    smallFont.Print(spriteBatch, "DROP");
                    break;

                case ItemMenu.States.EquipSelected1:
                case ItemMenu.States.EquipSelected2:
                    smallFont.Print(spriteBatch, "EQUIP");
                    break;

                case ItemMenu.States.GiveSelected1:
                case ItemMenu.States.GiveSelected2:
                    smallFont.Print(spriteBatch, "GIVE");
                    break;

                case ItemMenu.States.UseSelected1:
                case ItemMenu.States.UseSelected2:
                    smallFont.Print(spriteBatch, "USE");
                    break;
            }
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            spriteBatch.End();
        }

        private void Draw_GeneralMenuMoveOutMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, map.Position, false, false);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            generalMenu.DrawAt(spriteBatch, tempMenuPosition);
            spriteBatch.End();
        }

        private void Draw_GeneralMenuMoveInMode(GameTime gameTime)
        {
            Draw_GeneralMenuMoveOutMode(gameTime);
        }

        private void Draw_BattleMenuMoveOutMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            battleMenu.DrawAt(spriteBatch, tempMenuPosition);
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            spriteBatch.End();
        }

        private void Draw_BattleMenuMoveInMode(GameTime gameTime)
        {
            Draw_BattleMenuMoveOutMode(gameTime);
        }

        private void Draw_ItemMenuMoveOutMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            itemMenu.DrawAt(spriteBatch, tempMenuPosition);
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player); 
            spriteBatch.End();
        }

        private void Draw_ItemMenuMoveInMode(GameTime gameTime)
        {
            Draw_ItemMenuMoveOutMode(gameTime);
        }

        private void Draw_ItemMagicSelectionMenuMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, true);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            itemMagicSelectionMenu.Draw(spriteBatch);
            captionBox.Draw(spriteBatch);
            if (battleMenu.CurrentState == BattleMenu.States.ItemSelected1 || battleMenu.CurrentState == BattleMenu.States.ItemSelected2)
            {
                Draw_Items();
                smallFont.Position = captionBox.Position + new Vector2(20, 16);
                smallFont.Println(spriteBatch, party.Members[party.MemberOnTurn].Items[selectedItemNumber].Name1);
                smallFont.Println(spriteBatch, party.Members[party.MemberOnTurn].Items[selectedItemNumber].Name2);
                if (party.Members[party.MemberOnTurn].ItemEquipped[selectedItemNumber])
                    smallFont.Print(spriteBatch, "Equipped", new Color(231, 130, 66));
            }
            else if (battleMenu.CurrentState == BattleMenu.States.MagicSelected1 || battleMenu.CurrentState == BattleMenu.States.MagicSelected2)
            {
                Draw_Magic();
                smallFont.Position = captionBox.Position + new Vector2(20, 16);
                smallFont.Println(spriteBatch, party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].Name);
                smallFont.Println(spriteBatch, "");
                smallFont.Print(spriteBatch, "MP" + RightAlign(itoa(party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].MagicPoints[party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].Level - 1]), 4));
                for(int i = 0; i < party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].Level; i++)
                    spriteBatch.Draw(spellLevelSelectedTexture, captionBox.Position + new Vector2(20, 16) + new Vector2(-2, 18) + new Vector2(30, 0) * i, Color.White);
            }
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            spriteBatch.End();
        }

        private void Draw_ItemMagicSelectionMenuMoveInMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            itemMagicSelectionMenu.DrawAt(spriteBatch, tempMenuPosition);
            if (battleMenu.CurrentState == BattleMenu.States.ItemSelected1 || battleMenu.CurrentState == BattleMenu.States.ItemSelected2)
                Draw_Items();
            else if (battleMenu.CurrentState == BattleMenu.States.MagicSelected1 || battleMenu.CurrentState == BattleMenu.States.MagicSelected2)
                Draw_Magic();
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            spriteBatch.End();
        }

        private void Draw_ItemMagicSelectionMenuMoveOutMode(GameTime gameTime)
        {
            Draw_ItemMagicSelectionMenuMoveInMode(gameTime);
        }

        private void Draw_DisplayTextMessageMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            if (map != null)
               map.Draw(spriteBatch, true);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            if (enemies != null)
                enemies.Draw(spriteBatch, map);
            textMessageBox.Draw(spriteBatch);
            Draw_TextMessage();
            if (positionInTextMessage.Column == textMessage[positionInTextMessage.Row].Length - 1 && positionInTextMessage.Row == 2 && positionInTextMessage.Row < textMessageLength - 1 && textMessageCounter == 0)
                arrowforward.Draw(spriteBatch);
            if (story_character != null)
            {
                switch(story_character)
                {
                    case "JACOB":
                        portraitBox.Draw(spriteBatch);
                        spriteBatch.Draw(Character.TexturesJacob_Portrait, portraitBox.Position + new Vector2(20, 15), Color.White);
                        break;

                    case "CARYN":
                        portraitBox.Draw(spriteBatch);
                        spriteBatch.Draw(Character.TexturesCaryn_Portrait, portraitBox.Position + new Vector2(20, 15), Color.White);
                        break;

                    case "TASSI":
                        portraitBox.Draw(spriteBatch);
                        spriteBatch.Draw(Character.TexturesTassi_Portrait, portraitBox.Position + new Vector2(20, 15), Color.White);
                        break;

                    case "EVA":
                        portraitBox.Draw(spriteBatch);
                        spriteBatch.Draw(Character.TexturesEva_Portrait, portraitBox.Position + new Vector2(20, 15), Color.White);
                        break;

                    case "HANS":
                        portraitBox.Draw(spriteBatch);
                        spriteBatch.Draw(Character.TexturesHans_Portrait, portraitBox.Position + new Vector2(20, 15), Color.White);
                        break;

                    case "MONICA":
                        portraitBox.Draw(spriteBatch);
                        spriteBatch.Draw(Character.TexturesMonica_Portrait, portraitBox.Position + new Vector2(20, 15), Color.White);
                        break;

                    default:
                        break;
                }
            }
            spriteBatch.End();
        }

        private void Draw_DisplayTextMessageMoveOutMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            if (map != null)
                map.Draw(spriteBatch);
            Blink(gameTime);
            if (party != null)
                party.Draw(spriteBatch, map);
            if (enemies != null)
                enemies.Draw(spriteBatch, map);
            textMessageBox.DrawAt(spriteBatch, tempTextMessageBoxPosition);
            Draw_TextMessage(tempTextMessageBoxPosition);
            spriteBatch.End();
        }

        private void Draw_DropItemMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            textMessageBox.Draw(spriteBatch);
            Draw_TextMessage();
            yesNoButtons.Draw(spriteBatch);
            captionBox.Draw(spriteBatch);
            smallFont.Position = captionBox.Position + new Vector2(20, 16);
            if (yesNoButtons.CurrentState == YesNoButtons.States.YesSelected1 || yesNoButtons.CurrentState == YesNoButtons.States.YesSelected2)
                smallFont.Print(spriteBatch, "Yes");
            else if (yesNoButtons.CurrentState == YesNoButtons.States.NoSelected1 || yesNoButtons.CurrentState == YesNoButtons.States.NoSelected2)
                smallFont.Print(spriteBatch, "No");
            spriteBatch.End();
        }

        private void Draw_GiveItemMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, true);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            selectionBar.Draw(spriteBatch, oldSelectionBarPosition);
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            spriteBatch.End();
        }

        private void Draw_SelectionBarTransitionInGiveItemMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, true);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            selectionBar.DrawAt(spriteBatch, tempSelectionBarPosition);
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            spriteBatch.End();
        }

        private void Draw_SelectionBarTransitionOutGiveItemMode(GameTime gameTime)
        {
            Draw_SelectionBarTransitionInGiveItemMode(gameTime);
        }

        private void Draw_SwapItemMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, true);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            swapMenu.Draw(spriteBatch);
            captionBox.Draw(spriteBatch);
            Draw_Items_SwapMenu();
            smallFont.Position = captionBox.Position + new Vector2(20, 16);
            smallFont.Println(spriteBatch, party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].Items[selectedItemNumber].Name1);
            smallFont.Println(spriteBatch, party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].Items[selectedItemNumber].Name2);
            if (party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].ItemEquipped[selectedItemNumber])
                smallFont.Print(spriteBatch, "Equipped", new Color(231, 130, 66));
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            spriteBatch.End();
        }

        private void Draw_SwapItemMoveInMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            swapMenu.DrawAt(spriteBatch, tempMenuPosition);
            Draw_Items_SwapMenu();
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            spriteBatch.End();
        }

        private void Draw_SwapItemMoveOutMode(GameTime gameTime)
        {
            Draw_SwapItemMoveInMode(gameTime);
        }

        private void Draw_EquipWeaponMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, true);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            equipWeaponMenu.Draw(spriteBatch);
            captionBox.Draw(spriteBatch);
            Draw_Items_EquipWeaponMenu();
            smallFont.Position = captionBox.Position + new Vector2(20, 16);
            if (weapons[selectedWeaponNumber] != -1)
            {
                smallFont.Println(spriteBatch, party.Members[party.MemberOnTurn].Items[weapons[selectedWeaponNumber]].Name1);
                smallFont.Println(spriteBatch, party.Members[party.MemberOnTurn].Items[weapons[selectedWeaponNumber]].Name2);
            }
            else
            {
                smallFont.Println(spriteBatch, nothing.Name1);
                smallFont.Print(spriteBatch, nothing.Name2, new Color(231, 130, 66));
            }
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            Draw_EquipSpecsBox();
            spriteBatch.End();
        }

        private void Draw_EquipWeaponMoveInMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            equipWeaponMenu.DrawAt(spriteBatch, tempMenuPosition);
            Draw_Items_EquipWeaponMenu();
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            spriteBatch.End();
        }

        private void Draw_EquipWeaponMoveOutMode(GameTime gameTime)
        {
            Draw_EquipWeaponMoveInMode(gameTime);
        }

        private void Draw_EquipRingMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, true);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            equipRingMenu.Draw(spriteBatch);
            captionBox.Draw(spriteBatch);
            Draw_Items_EquipRingMenu();
            smallFont.Position = captionBox.Position + new Vector2(20, 16);
            if (rings[selectedRingNumber] != -1)
            {
                smallFont.Println(spriteBatch, party.Members[party.MemberOnTurn].Items[rings[selectedRingNumber]].Name1);
                smallFont.Println(spriteBatch, party.Members[party.MemberOnTurn].Items[rings[selectedRingNumber]].Name2);
            }
            else
            {
                smallFont.Println(spriteBatch, nothing.Name1);
                smallFont.Print(spriteBatch, nothing.Name2, new Color(231, 130, 66));
            }
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            Draw_EquipSpecsBox();
            spriteBatch.End();
        }

        private void Draw_EquipRingMoveInMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            equipRingMenu.DrawAt(spriteBatch, tempMenuPosition);
            Draw_Items_EquipRingMenu();
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            spriteBatch.End();
        }

        private void Draw_EquipRingMoveOutMode(GameTime gameTime)
        {
            Draw_EquipRingMoveInMode(gameTime);
        }

        private void Draw_PlayerBattleMode(GameTime gameTime)
        {
            // *** temporary solution until battle screens are available
            spriteBatch.Begin();
            map.Draw(spriteBatch);
            Blink(gameTime);
            enemies.Draw(spriteBatch, map);
            party.Draw(spriteBatch, map);
            party.Members[party.MemberOnTurn].Draw(spriteBatch, map.CalcPosition(party.Members[party.MemberOnTurn].Position));
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            spriteBatch.End();
        }

        private void Draw_DisplayDefeatedCharactersMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch);
            Blink(gameTime);
            if (displayDefeatedCharactersModeState < DEFEAT_DISPLAY_WAIT_DURATION)
                enemies.Draw(spriteBatch, map);
            else
                for (int i = 0; i < enemies.NumPartyMembers; i++)
                {
                    if (enemies.Members[i].Alive)
                    {
                        if (enemies.Members[i].HitPoints == 0)
                            spriteBatch.Draw(explosionTexture[(displayDefeatedCharactersModeState - DEFEAT_DISPLAY_WAIT_DURATION) / DEFEAT_DISPLAY_EXPLOSION_STEP_DURATION], map.CalcPosition(enemies.Members[i].Position) * new Vector2(Map.TILESIZEX, Map.TILESIZEY), Color.White);
                        else
                            enemies.Members[i].Draw(spriteBatch, map.CalcPosition(enemies.Members[i].Position));
                    }
                }
            if (displayDefeatedCharactersModeState < DEFEAT_DISPLAY_WAIT_DURATION)
                party.Draw(spriteBatch, map);
            else
                for (int i = 0; i < party.NumPartyMembers; i++)
                {
                    if (party.Members[i].Alive)
                    {
                        if (party.Members[i].HitPoints == 0)
                            spriteBatch.Draw(explosionTexture[(displayDefeatedCharactersModeState - DEFEAT_DISPLAY_WAIT_DURATION) / DEFEAT_DISPLAY_EXPLOSION_STEP_DURATION], map.CalcPosition(party.Members[i].Position) * new Vector2(Map.TILESIZEX, Map.TILESIZEY), Color.White);
                        else
                            party.Members[i].Draw(spriteBatch, map.CalcPosition(party.Members[i].Position));
                    }
                }
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            switch (characterPointer.BelongsToSide)
            {
                case CharacterPointer.Sides.Player:
                    Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
                    break;

                case CharacterPointer.Sides.CPU_Opponents:
                    Draw_CharacterStatsBox(enemies.Members[enemies.MemberOnTurn], CharacterPointer.Sides.CPU_Opponents);
                    break;
            }
            spriteBatch.End();
        }

        private void Draw_NextCharacterAfterSleepMode(GameTime gameTime)
        {
            Draw_PlayerMoveMode(gameTime);
        }

        private void Draw_PlayerBattleDisplayTextMessageMode(GameTime gameTime)
        {
            // *** temporary solution until battle screens are available
            spriteBatch.Begin();
            map.Draw(spriteBatch, map.Position, true, false);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            textMessageBox.Draw(spriteBatch);
            Draw_TextMessage();
            if (positionInTextMessage.Column == textMessage[positionInTextMessage.Row].Length - 1 && positionInTextMessage.Row == 2 && positionInTextMessage.Row < textMessageLength - 1 && textMessageCounter == 0)
                arrowforward.Draw(spriteBatch);
            spriteBatch.End();
        }

        private void Draw_BattleDisplayTextMessageMoveOutMode(GameTime gameTime)
        {
            Draw_DisplayTextMessageMoveOutMode(gameTime);
        }

        private void Draw_MagicLevelSelectionMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, true);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            itemMagicSelectionMenu.Draw(spriteBatch);
            captionBox.Draw(spriteBatch);
            Draw_Magic();
            smallFont.Position = captionBox.Position + new Vector2(20, 16);
            smallFont.Println(spriteBatch, party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].Name);
            smallFont.Println(spriteBatch, "");
            smallFont.Print(spriteBatch, "MP" + RightAlign(itoa(party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].MagicPoints[selectedMagicLevel - 1]), 4));
            for (int i = 0; i < selectedMagicLevel; i++)
                spriteBatch.Draw(spellLevelSelectedTexture, captionBox.Position + new Vector2(20, 16) + new Vector2(-2, 18) + new Vector2(30, 0) * i, Color.White);
            for (int i = selectedMagicLevel; i < party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].Level; i++)
                spriteBatch.Draw(spellLevelUnselectedTexture, captionBox.Position + new Vector2(20, 16) + new Vector2(-2, 18) + new Vector2(30, 0) * i, Color.White);
            redbar.Draw(spriteBatch);
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            spriteBatch.End();
        }

        private void Draw_EnemyMoveTransitionMode(GameTime gameTime)
        {
            Draw_PlayerMoveTransitionMode(gameTime);
        }

        private void Draw_TransitionInEnemyMoveTransitionMode(GameTime gameTime)
        {
            Draw_TransitionInPlayerMoveTransitionMode(gameTime);
        }

        private void Draw_EnemyMoveMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch);
            Blink(gameTime);
            enemies.Draw(spriteBatch, map);
            party.Draw(spriteBatch, map);
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(enemies.Members[enemies.MemberOnTurn], CharacterPointer.Sides.CPU_Opponents);
            spriteBatch.End();
        }

        private void Draw_EnemyMovingMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch);
            Blink(gameTime);
            party.DrawAt(spriteBatch, map, map.Position, false, map.Offset);
            enemies.DrawAt(spriteBatch, map, map.Position, true, map.Offset);
            enemies.Members[enemies.MemberOnTurn].DrawAt(spriteBatch, delayPosition + new Vector2(Character.OFFSETX, Character.OFFSETY));
            Draw_LandEffectBox(enemies.Members[enemies.MemberOnTurn].Position);
            Draw_CharacterStatsBox(enemies.Members[enemies.MemberOnTurn], CharacterPointer.Sides.CPU_Opponents);
            spriteBatch.End();
        }

        private void Draw_EnemyBattleMode(GameTime gameTime)
        {
            // *** temporary solution until battle screens are available
            spriteBatch.Begin();
            map.Draw(spriteBatch, map.Position, true, false);
            Blink(gameTime);
            enemies.Draw(spriteBatch, map);
            party.Draw(spriteBatch, map);
            enemies.Members[enemies.MemberOnTurn].Draw(spriteBatch, map.CalcPosition(enemies.Members[enemies.MemberOnTurn].Position));
            Draw_LandEffectBox(enemies.Members[enemies.MemberOnTurn].Position);
            Draw_CharacterStatsBox(enemies.Members[enemies.MemberOnTurn], CharacterPointer.Sides.CPU_Opponents);
            spriteBatch.End();
        }

        private void Draw_EnemyBattleDisplayTextMessageMode(GameTime gameTime)
        {
            Draw_PlayerBattleDisplayTextMessageMode(gameTime);
        }

        private void Draw_EnemyAttackMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, oldMapPosition, true, false);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            // **** to be tested
            switch (enemyBattleMove)
            {
                case BattleMoves.MagicAttack:
                case BattleMoves.MagicHeal:
                    switch (enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].Area[selectedMagicLevel - 1])
                    {
                        case 1:
                            selectionBar.Draw(spriteBatch, oldSelectionBarPosition);
                            break;

                        case 2:
                            selectionBar2.DrawAt(spriteBatch, new Vector2((oldSelectionBarPosition.X - 1) * Map.TILESIZEX, (oldSelectionBarPosition.Y - 1) * Map.TILESIZEY) + new Vector2(SelectionBar.OFFSETX, SelectionBar.OFFSETY));
                            break;

                        case 3:
                            selectionBar3.DrawAt(spriteBatch, new Vector2((oldSelectionBarPosition.X - 2) * Map.TILESIZEX, (oldSelectionBarPosition.Y - 2) * Map.TILESIZEY) + new Vector2(SelectionBar.OFFSETX, SelectionBar.OFFSETY));
                            break;
                    }
                    break;

                case BattleMoves.ItemMagicAttack:
                case BattleMoves.ItemMagicHeal:
                    switch (enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Area[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1])
                    {
                        case 1:
                            selectionBar.Draw(spriteBatch, oldSelectionBarPosition);
                            break;

                        case 2:
                            selectionBar2.DrawAt(spriteBatch, new Vector2((oldSelectionBarPosition.X - 1) * Map.TILESIZEX, (oldSelectionBarPosition.Y - 1) * Map.TILESIZEY) + new Vector2(SelectionBar.OFFSETX, SelectionBar.OFFSETY));
                            break;

                        case 3:
                            selectionBar3.DrawAt(spriteBatch, new Vector2((oldSelectionBarPosition.X - 2) * Map.TILESIZEX, (oldSelectionBarPosition.Y - 2) * Map.TILESIZEY) + new Vector2(SelectionBar.OFFSETX, SelectionBar.OFFSETY));
                            break;
                    }
                    break;

                default:
                    selectionBar.Draw(spriteBatch, oldSelectionBarPosition);
                    break;
            }
            Draw_LandEffectBox(enemies.Members[enemies.MemberOnTurn].Position);
            // *** this has to be adapted to the original game
            Draw_CharacterStatsBox(enemies.Members[enemies.MemberOnTurn], CharacterPointer.Sides.CPU_Opponents);
            Draw_TargetCharacterStatsBox();
            spriteBatch.End();
        }

        private void Draw_TransitionInEnemyAttackMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, map.Position, true, false);
            Blink(gameTime);
            party.DrawAt(spriteBatch, map, map.Position, false, map.Offset);
            enemies.DrawAt(spriteBatch, map, map.Position, false, map.Offset);
            // **** to be tested
            switch (enemyBattleMove)
            {
                case BattleMoves.MagicAttack:
                case BattleMoves.MagicHeal:
                    switch (enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].Area[selectedMagicLevel - 1])
                    {
                        case 1:
                            selectionBar.DrawAt(spriteBatch, tempSelectionBarPosition);
                            break;

                        case 2:
                            selectionBar2.DrawAt(spriteBatch, tempSelectionBarPosition - new Vector2(Map.TILESIZEX, Map.TILESIZEY));
                            break;

                        case 3:
                            selectionBar3.DrawAt(spriteBatch, tempSelectionBarPosition - new Vector2(Map.TILESIZEX * 2, Map.TILESIZEY * 2));
                            break;
                    }
                    break;

                case BattleMoves.ItemMagicAttack:
                case BattleMoves.ItemMagicHeal:
                    switch (enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Area[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1])
                    {
                        case 1:
                            selectionBar.DrawAt(spriteBatch, tempSelectionBarPosition);
                            break;

                        case 2:
                            selectionBar2.DrawAt(spriteBatch, tempSelectionBarPosition - new Vector2(Map.TILESIZEX, Map.TILESIZEY));
                            break;

                        case 3:
                            selectionBar3.DrawAt(spriteBatch, tempSelectionBarPosition - new Vector2(Map.TILESIZEX * 2, Map.TILESIZEY * 2));
                            break;
                    }
                    break;

                default:
                    selectionBar.DrawAt(spriteBatch, tempSelectionBarPosition);
                    break;
            }
            Draw_LandEffectBox(enemies.Members[enemies.MemberOnTurn].Position);
            // *** this has to be adapted to the original game
            Draw_CharacterStatsBox(enemies.Members[enemies.MemberOnTurn], CharacterPointer.Sides.CPU_Opponents);
            Draw_TargetCharacterStatsBox();
            spriteBatch.End();
        }

        private void Draw_HealMenuMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, oldMapPosition, true);
            Blink(gameTime);
            party.Draw(spriteBatch, map);
            enemies.Draw(spriteBatch, map);
            switch (battleMenu.CurrentState)
            {
                case BattleMenu.States.MagicSelected1:
                case BattleMenu.States.MagicSelected2:
                    switch (party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].Area[selectedMagicLevel - 1])
                    {
                        case 1:
                            selectionBar.Draw(spriteBatch, oldSelectionBarPosition);
                            break;

                        case 2:
                            selectionBar2.DrawAt(spriteBatch, new Vector2((oldSelectionBarPosition.X - 1) * Map.TILESIZEX, (oldSelectionBarPosition.Y - 1) * Map.TILESIZEY) + new Vector2(SelectionBar.OFFSETX, SelectionBar.OFFSETY));
                            break;

                        case 3:
                            selectionBar3.DrawAt(spriteBatch, new Vector2((oldSelectionBarPosition.X - 2) * Map.TILESIZEX, (oldSelectionBarPosition.Y - 2) * Map.TILESIZEY) + new Vector2(SelectionBar.OFFSETX, SelectionBar.OFFSETY));
                            break;
                    }
                    break;

                case BattleMenu.States.ItemSelected1:
                case BattleMenu.States.ItemSelected2:
                    switch (party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Area[party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1])
                    {
                        case 1:
                            selectionBar.Draw(spriteBatch, oldSelectionBarPosition);
                            break;

                        case 2:
                            selectionBar2.DrawAt(spriteBatch, new Vector2((oldSelectionBarPosition.X - 1) * Map.TILESIZEX, (oldSelectionBarPosition.Y - 1) * Map.TILESIZEY) + new Vector2(SelectionBar.OFFSETX, SelectionBar.OFFSETY));
                            break;

                        case 3:
                            selectionBar3.DrawAt(spriteBatch, new Vector2((oldSelectionBarPosition.X - 2) * Map.TILESIZEX, (oldSelectionBarPosition.Y - 2) * Map.TILESIZEY) + new Vector2(SelectionBar.OFFSETX, SelectionBar.OFFSETY));
                            break;
                    }
                    break;

                default:
                    selectionBar.Draw(spriteBatch, oldSelectionBarPosition);
                    break;
            }
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            Draw_SelectedCharacterStatsBox();
            spriteBatch.End();
        }

        private void Draw_TransitionInHealMenuMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, true);
            Blink(gameTime);
            party.DrawAt(spriteBatch, map, map.Position, false, map.Offset);
            enemies.DrawAt(spriteBatch, map, map.Position, false, map.Offset);
            switch (battleMenu.CurrentState)
            {
                case BattleMenu.States.MagicSelected1:
                case BattleMenu.States.MagicSelected2:
                    switch (party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].Area[selectedMagicLevel - 1])
                    {
                        case 1:
                            selectionBar.DrawAt(spriteBatch, tempSelectionBarPosition);
                            break;

                        case 2:
                            selectionBar2.DrawAt(spriteBatch, tempSelectionBarPosition - new Vector2(Map.TILESIZEX, Map.TILESIZEY));
                            break;

                        case 3:
                            selectionBar3.DrawAt(spriteBatch, tempSelectionBarPosition - new Vector2(Map.TILESIZEX * 2, Map.TILESIZEY * 2));
                            break;
                    }
                    break;

                case BattleMenu.States.ItemSelected1:
                case BattleMenu.States.ItemSelected2:
                    switch (party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Area[party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1])
                    {
                        case 1:
                            selectionBar.DrawAt(spriteBatch, tempSelectionBarPosition);
                            break;

                        case 2:
                            selectionBar2.DrawAt(spriteBatch, tempSelectionBarPosition - new Vector2(Map.TILESIZEX, Map.TILESIZEY));
                            break;

                        case 3:
                            selectionBar3.DrawAt(spriteBatch, tempSelectionBarPosition - new Vector2(Map.TILESIZEX * 2, Map.TILESIZEY * 2));
                            break;
                    }
                    break;

                default:
                    selectionBar.DrawAt(spriteBatch, tempSelectionBarPosition);
                    break;
            }
            Draw_LandEffectBox(party.Members[party.MemberOnTurn].Position);
            Draw_CharacterStatsBox(party.Members[party.MemberOnTurn], CharacterPointer.Sides.Player);
            Draw_SelectedCharacterStatsBox();
            spriteBatch.End();
        }

        private void Draw_EndTurnOfCharacterAndNextCharacterMode(GameTime gameTime)
        {
            switch (characterPointer.BelongsToSide)
            {
                case CharacterPointer.Sides.Player:
                    Draw_PlayerMoveMode(gameTime);
                    break;

                case CharacterPointer.Sides.CPU_Opponents:
                    Draw_EnemyMoveMode(gameTime);
                    break;
            }
        }

        private void Draw_PlayerAutoMoveMode(GameTime gameTime)
        {
            Draw_PlayerMoveMode(gameTime);
        }

        private void Draw_PlayerAutoAttackMode(GameTime gameTime)
        {
            Draw_AttackMenuMode(gameTime);
        }

        private void Draw_BattleFieldFadeOutMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, map.Position, false, false, fadingState);
            Blink(gameTime);
            party.DrawAt(spriteBatch, map, map.Position, false, map.Offset, fadingState);
            enemies.DrawAt(spriteBatch, map, map.Position, false, map.Offset, fadingState);
            spriteBatch.End();
        }

        private void Draw_BattleFieldFadeInMode(GameTime gameTime)
        {
            Draw_BattleFieldFadeOutMode(gameTime);
        }

        private void Draw_MemberMenuMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            map.Draw(spriteBatch, map.Position, false, false);
            Blink(gameTime);
            enemies.Draw(spriteBatch, map);
            party.Draw(spriteBatch, map);
            memberMenuDisplayStatsBox.Draw(spriteBatch);
            memberMenuCharacterSelectionBox.Draw(spriteBatch);
            portraitBox.Draw(spriteBatch);
            party.Members[memberMenuState_selectedMemberNumber].DrawPortraitAt(spriteBatch, portraitBox.Position + new Vector2(20, 15));
            smallFont.Position = memberMenuDisplayStatsBox.Position + new Vector2(20, 16);
            smallFont.Println(spriteBatch, party.Members[memberMenuState_selectedMemberNumber].Name + " " + party.Members[memberMenuState_selectedMemberNumber].CharClass + " L" + itoa(party.Members[memberMenuState_selectedMemberNumber].LevelToDisplay));
            smallFont.Println(spriteBatch, "");
            smallFont.Println(spriteBatch, "MAGIC     ITEM");
            if (party.Members[memberMenuState_selectedMemberNumber].MagicSpells == null)
                smallFont.Print(spriteBatch, " Nothing", new Color(231, 130, 66));
            else
                for (int i = 0; i < Character.MAXNUMBER_SPELLS; i++)
                    if (party.Members[memberMenuState_selectedMemberNumber].MagicSpells[i] != null)
                    {
                        smallFont.Println(spriteBatch, " " + party.Members[memberMenuState_selectedMemberNumber].MagicSpells[i].Name);
                        smallFont.Println(spriteBatch, "  Level " + itoa(party.Members[memberMenuState_selectedMemberNumber].MagicSpells[i].Level));
                    }
            smallFont.Position = new Vector2(memberMenuDisplayStatsBox.Position.X + 11 * smallFont.Size.X, memberMenuDisplayStatsBox.Position.Y + 4 * smallFont.Size.Y);
            if (party.Members[memberMenuState_selectedMemberNumber].Items == null)
                smallFont.Print(spriteBatch, " Nothing", new Color(231, 130, 66));
            else
                for (int i = 0; i < Character.MAXNUMBER_ITEMS; i++)
                    if (party.Members[memberMenuState_selectedMemberNumber].Items[i] != null)
                    {
                        if (party.Members[memberMenuState_selectedMemberNumber].ItemEquipped != null && party.Members[memberMenuState_selectedMemberNumber].ItemEquipped[i])
                            smallFont.Print(spriteBatch, "@");
                        smallFont.Println(spriteBatch, " " + party.Members[memberMenuState_selectedMemberNumber].Items[i].Name1);
                        smallFont.Println(spriteBatch, " " + party.Members[memberMenuState_selectedMemberNumber].Items[i].Name2);
                    }
            smallFont.Position = memberMenuCharacterSelectionBox.Position + new Vector2(28, 16);
            smallFont.Println(spriteBatch, " NAME");
            for (int i = 0; i < MEMBERMENU_NUMCHARSPERPAGE && memberMenuState_firstMemberNumberToDisplay + i < party.NumPartyMembers; i++)
            {
                smallFont.Println(spriteBatch, "");
                smallFont.Print(spriteBatch, "^", Color.White);
                smallFont.Println(spriteBatch, " " + party.Members[memberMenuState_firstMemberNumberToDisplay + i].Name, party.Members[memberMenuState_firstMemberNumberToDisplay + i].Alive ? Color.White : new Color(231, 130, 66));
            }
            smallFont.Position = memberMenuCharacterSelectionBox.Position + new Vector2(28, 16) + new Vector2(9 * smallFont.Size.X, 0);
            if (memberMenuState_displayDetails)
            {
                smallFont.Println(spriteBatch, "HP");
                for (int i = 0; i < MEMBERMENU_NUMCHARSPERPAGE && memberMenuState_firstMemberNumberToDisplay + i < party.NumPartyMembers; i++)
                {
                    smallFont.Println(spriteBatch, "");
                    smallFont.Println(spriteBatch, RightAlign(itoa(party.Members[memberMenuState_firstMemberNumberToDisplay + i].HitPoints), 2), party.Members[memberMenuState_firstMemberNumberToDisplay + i].Alive ? Color.White : new Color(231, 130, 66));
                }
                smallFont.Position = memberMenuCharacterSelectionBox.Position + new Vector2(28, 16) + new Vector2(12 * smallFont.Size.X, 0);
                smallFont.Println(spriteBatch, "MP");
                for (int i = 0; i < MEMBERMENU_NUMCHARSPERPAGE && memberMenuState_firstMemberNumberToDisplay + i < party.NumPartyMembers; i++)
                {
                    smallFont.Println(spriteBatch, "");
                    smallFont.Println(spriteBatch, RightAlign(itoa(party.Members[memberMenuState_firstMemberNumberToDisplay + i].MagicPoints), 2), party.Members[memberMenuState_firstMemberNumberToDisplay + i].Alive ? Color.White : new Color(231, 130, 66));
                }
                smallFont.Position = memberMenuCharacterSelectionBox.Position + new Vector2(28, 16) + new Vector2(15 * smallFont.Size.X, 0);
                smallFont.Println(spriteBatch, "AT");
                for (int i = 0; i < MEMBERMENU_NUMCHARSPERPAGE && memberMenuState_firstMemberNumberToDisplay + i < party.NumPartyMembers; i++)
                {
                    smallFont.Println(spriteBatch, "");
                    smallFont.Println(spriteBatch, RightAlign(itoa(party.Members[memberMenuState_firstMemberNumberToDisplay + i].AttackPoints), 2), party.Members[memberMenuState_firstMemberNumberToDisplay + i].Alive ? Color.White : new Color(231, 130, 66));
                }
                smallFont.Position = memberMenuCharacterSelectionBox.Position + new Vector2(28, 16) + new Vector2(18 * smallFont.Size.X, 0);
                smallFont.Println(spriteBatch, "DF");
                for (int i = 0; i < MEMBERMENU_NUMCHARSPERPAGE && memberMenuState_firstMemberNumberToDisplay + i < party.NumPartyMembers; i++)
                {
                    smallFont.Println(spriteBatch, "");
                    smallFont.Println(spriteBatch, RightAlign(itoa(party.Members[memberMenuState_firstMemberNumberToDisplay + i].DefensePoints), 2), party.Members[memberMenuState_firstMemberNumberToDisplay + i].Alive ? Color.White : new Color(231, 130, 66));
                }
                smallFont.Position = memberMenuCharacterSelectionBox.Position + new Vector2(28, 16) + new Vector2(21 * smallFont.Size.X, 0);
                smallFont.Println(spriteBatch, "AG");
                for (int i = 0; i < MEMBERMENU_NUMCHARSPERPAGE && memberMenuState_firstMemberNumberToDisplay + i < party.NumPartyMembers; i++)
                {
                    smallFont.Println(spriteBatch, "");
                    smallFont.Println(spriteBatch, RightAlign(itoa(party.Members[memberMenuState_firstMemberNumberToDisplay + i].Agility), 2), party.Members[memberMenuState_firstMemberNumberToDisplay + i].Alive ? Color.White : new Color(231, 130, 66));
                }
                smallFont.Position = memberMenuCharacterSelectionBox.Position + new Vector2(28, 16) + new Vector2(24 * smallFont.Size.X, 0);
                smallFont.Println(spriteBatch, "MV");
                for (int i = 0; i < MEMBERMENU_NUMCHARSPERPAGE && memberMenuState_firstMemberNumberToDisplay + i < party.NumPartyMembers; i++)
                {
                    smallFont.Println(spriteBatch, "");
                    smallFont.Println(spriteBatch, RightAlign(itoa(party.Members[memberMenuState_firstMemberNumberToDisplay + i].MovePoints), 2), party.Members[memberMenuState_firstMemberNumberToDisplay + i].Alive ? Color.White : new Color(231, 130, 66));
                }
            }
            else
            {
                smallFont.Println(spriteBatch, "CLASS");
                for (int i = 0; i < MEMBERMENU_NUMCHARSPERPAGE && memberMenuState_firstMemberNumberToDisplay + i < party.NumPartyMembers; i++)
                {
                    smallFont.Println(spriteBatch, "");
                    smallFont.Println(spriteBatch, party.Members[memberMenuState_firstMemberNumberToDisplay + i].CharClass, party.Members[memberMenuState_firstMemberNumberToDisplay + i].Alive ? Color.White : new Color(231, 130, 66));
                }
                smallFont.Position = memberMenuCharacterSelectionBox.Position + new Vector2(28, 16) + new Vector2(19 * smallFont.Size.X, 0);
                smallFont.Println(spriteBatch, "LEV");
                for (int i = 0; i < MEMBERMENU_NUMCHARSPERPAGE && memberMenuState_firstMemberNumberToDisplay + i < party.NumPartyMembers; i++)
                {
                    smallFont.Println(spriteBatch, "");
                    smallFont.Println(spriteBatch, RightAlign(itoa(party.Members[memberMenuState_firstMemberNumberToDisplay + i].LevelToDisplay), 3), party.Members[memberMenuState_firstMemberNumberToDisplay + i].Alive ? Color.White : new Color(231, 130, 66));
                }
                smallFont.Position = memberMenuCharacterSelectionBox.Position + new Vector2(28, 16) + new Vector2(23 * smallFont.Size.X, 0);
                smallFont.Println(spriteBatch, "EXP");
                for (int i = 0; i < MEMBERMENU_NUMCHARSPERPAGE && memberMenuState_firstMemberNumberToDisplay + i < party.NumPartyMembers; i++)
                {
                    smallFont.Println(spriteBatch, "");
                    smallFont.Println(spriteBatch, RightAlign(itoa(party.Members[memberMenuState_firstMemberNumberToDisplay + i].ExperiencePoints), 3), party.Members[memberMenuState_firstMemberNumberToDisplay + i].Alive ? Color.White : new Color(231, 130, 66));
                }
            }
            if (memberMenuState_firstMemberNumberToDisplay > 0)
                arrowbackward.DrawAt(spriteBatch, memberMenuCharacterSelectionBox.Position + new Vector2(20, 16));
            if (memberMenuState_firstMemberNumberToDisplay + MEMBERMENU_NUMCHARSPERPAGE - 1 < party.NumPartyMembers - 1)
                arrowforward.DrawAt(spriteBatch, memberMenuCharacterSelectionBox.Position + new Vector2(0, memberMenuCharacterSelectionBox.Size.Y) + new Vector2(20, 16) + new Vector2(0, -40));
            redbar.DrawAt(spriteBatch, memberMenuCharacterSelectionBox.Position + new Vector2(28, 16) + new Vector2(smallFont.Size.X, (memberMenuState_selectedMemberNumber - memberMenuState_firstMemberNumberToDisplay + 1) * 2 * smallFont.Size.Y) - new Vector2(10, 8), 156, 32);
            spriteBatch.End();
        }

        private void Draw_LandEffectBox(float posXY)
        {
            landEffectBox.Draw(spriteBatch);
            smallFont.Position = new Vector2(landEffectBox.Position.X + 20, landEffectBox.Position.Y + 15);
            smallFont.Println(spriteBatch, @"LAND");
            smallFont.Println(spriteBatch, @"EFFECT");
            if(party.Members[party.MemberOnTurn].Flying)
                smallFont.Println(spriteBatch, @"    0%");
            else
                switch ((char) map.At(posXY))
                {
                    case 'F':
                        smallFont.Println(spriteBatch, @"   30%");
                        break;

                    case 'G':
                        smallFont.Println(spriteBatch, @"   15%");
                        break;

                    case 'H':
                        smallFont.Println(spriteBatch, @"   30%");
                        break;

                    case 'P':
                        smallFont.Println(spriteBatch, @"    0%");
                        break;

                    case 'S':
                        smallFont.Println(spriteBatch, @"    0%");
                        break;
                }
        }

        private void Draw_CharacterStatsBox(Character character, CharacterPointer.Sides side)
        {
            int characterStatsBoxWidth;
            int barsWidth;
            
            statsBarHitPoints.CalculateWidth(character.HitPoints, character.MaxHitPoints);
            statsBarMagicPoints.CalculateWidth(character.MagicPoints, character.MaxMagicPoints);

            if (side == CharacterPointer.Sides.Player)
                characterStatsBoxWidth = (int)((character.Name.Length + character.CharClass.Length + 3) * smallFont.Size.X);
            else
                characterStatsBoxWidth = (int)(character.Name.Length * smallFont.Size.X);

            barsWidth = statsBarHitPoints.TotalWidth;
            if (statsBarMagicPoints.TotalWidth > barsWidth)
                barsWidth = statsBarMagicPoints.TotalWidth;
            barsWidth += (int)smallFont.Size.X / 2;
            barsWidth += (int)smallFont.Size.X - (barsWidth % (int)smallFont.Size.X);
            if (barsWidth + 7 * (int)smallFont.Size.X > characterStatsBoxWidth)
                characterStatsBoxWidth = barsWidth + 7 * (int)smallFont.Size.X;
            
            characterStatsBox.Size = new Vector2(Box.Texture_TopLeft.Width + characterStatsBoxWidth + Box.Texture_TopRight.Width + 10, 80);
            // 2024.12.02 introduced constants
            characterStatsBox.Position = new Vector2(Game1.PREFERREDBACKBUFFERWIDTH - 30 - characterStatsBox.Size.X, characterStatsBox.Position.Y);
            characterStatsBox.Draw(spriteBatch);

            smallFont.Position = new Vector2(characterStatsBox.Position.X + 20, characterStatsBox.Position.Y + 15);
            smallFont.Print(spriteBatch, character.Name);
            if (side == CharacterPointer.Sides.Player)
            {
                smallFont.Position = new Vector2(characterStatsBox.Position.X + 20 + (character.Name.Length + 1) * smallFont.Size.X, characterStatsBox.Position.Y + 15);
                smallFont.Print(spriteBatch, character.CharClass);
                smallFont.Position = new Vector2(characterStatsBox.Position.X + 20 + (character.Name.Length + character.CharClass.Length + 1) * smallFont.Size.X, characterStatsBox.Position.Y + 15);
                smallFont.Print(spriteBatch, itoa(character.LevelToDisplay));
            }
            smallFont.Position = new Vector2(characterStatsBox.Position.X + 20, characterStatsBox.Position.Y + 15 + smallFont.Size.Y);
            smallFont.Print(spriteBatch, "HP");
            smallFont.Position = new Vector2(characterStatsBox.Position.X + 20 + barsWidth + 2 * smallFont.Size.X, characterStatsBox.Position.Y + 15 + smallFont.Size.Y);
            smallFont.Print(spriteBatch, RightAlign(itoa(character.HitPoints), 2));
            smallFont.Position = new Vector2(characterStatsBox.Position.X + 20 + barsWidth + 4 * smallFont.Size.X, characterStatsBox.Position.Y + 15 + smallFont.Size.Y);
            smallFont.Print(spriteBatch, "/");
            smallFont.Position = new Vector2(characterStatsBox.Position.X + 20 + barsWidth + 5 * smallFont.Size.X, characterStatsBox.Position.Y + 15 + smallFont.Size.Y);
            smallFont.Print(spriteBatch, RightAlign(itoa(character.MaxHitPoints), 2));
            smallFont.Position = new Vector2(characterStatsBox.Position.X + 20, characterStatsBox.Position.Y + 15 + 2 * smallFont.Size.Y);
            smallFont.Print(spriteBatch, "MP");
            smallFont.Position = new Vector2(characterStatsBox.Position.X + 20 + barsWidth + 2 * smallFont.Size.X, characterStatsBox.Position.Y + 15 + 2 * smallFont.Size.Y);
            smallFont.Print(spriteBatch, RightAlign(itoa(character.MagicPoints), 2));
            smallFont.Position = new Vector2(characterStatsBox.Position.X + 20 + barsWidth + 4 * smallFont.Size.X, characterStatsBox.Position.Y + 15 + 2 * smallFont.Size.Y);
            smallFont.Print(spriteBatch, "/");
            smallFont.Position = new Vector2(characterStatsBox.Position.X + 20 + barsWidth + 5 * smallFont.Size.X, characterStatsBox.Position.Y + 15 + 2 * smallFont.Size.Y);
            smallFont.Print(spriteBatch, RightAlign(itoa(character.MaxMagicPoints), 2));

            statsBarHitPoints.Position = new Vector2(characterStatsBox.Position.X + 20 + 2 * smallFont.Size.X, characterStatsBox.Position.Y + 15 + smallFont.Size.Y);
            statsBarHitPoints.Draw(spriteBatch);
            statsBarMagicPoints.Position = new Vector2(characterStatsBox.Position.X + 20 + 2 * smallFont.Size.X, characterStatsBox.Position.Y + 15 + 2 * smallFont.Size.Y);
            statsBarMagicPoints.Draw(spriteBatch);
        }

        private void Draw_TargetCharacterStatsBox()
        {
            Vector2 backUpCharacterStatsBoxSize = characterStatsBox.Size;
            Vector2 backUpCharacterStatsBoxPosition = characterStatsBox.Position;
            // 2024.12.02 introduced constants
            characterStatsBox.Position = new Vector2(characterStatsBox.Position.X, Game1.PREFERREDBACKBUFFERHEIGHT - 30 - characterStatsBox.Size.Y);
            switch (target.BelongsToSide)
            {
                case CharacterPointer.Sides.CPU_Opponents:
                    Draw_CharacterStatsBox(enemies.Members[target.WhichOne], target.BelongsToSide);
                    break;

                case CharacterPointer.Sides.Player:
                    Draw_CharacterStatsBox(party.Members[target.WhichOne], target.BelongsToSide);
                    break;
            }
            characterStatsBox.Size = backUpCharacterStatsBoxSize;
            characterStatsBox.Position = backUpCharacterStatsBoxPosition;
        }

        private void Draw_SelectedCharacterStatsBox()
        {
            Vector2 backUpCharacterStatsBoxSize = characterStatsBox.Size;
            Vector2 backUpCharacterStatsBoxPosition = characterStatsBox.Position;
            characterStatsBox.Size = new Vector2(Box.Texture_TopLeft.Width + (party.Members[map.CharacterLocatedAt(selectionBar.PositionInMap(map), party)].CharClass.Length + party.Members[map.CharacterLocatedAt(selectionBar.PositionInMap(map), party)].Name.Length + 3) * smallFont.Size.X + Box.Texture_TopRight.Width + 10, 80);
            // 2024.12.02 introduced constants
            characterStatsBox.Position = new Vector2(Game1.PREFERREDBACKBUFFERWIDTH - 30 - characterStatsBox.Size.X, Game1.PREFERREDBACKBUFFERHEIGHT - 30 - characterStatsBox.Size.Y);
            Draw_CharacterStatsBox(party.Members[map.CharacterLocatedAt(selectionBar.PositionInMap(map), party)], CharacterPointer.Sides.Player);
            characterStatsBox.Size = backUpCharacterStatsBoxSize;
            characterStatsBox.Position = backUpCharacterStatsBoxPosition;
        }

        private void Draw_StoryMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            textMessageBox.Draw(spriteBatch);
            Draw_TextMessage();
            if (positionInTextMessage.Column == textMessage[positionInTextMessage.Row].Length - 1 && positionInTextMessage.Row == 2 && positionInTextMessage.Row < textMessageLength - 1 && textMessageCounter == 0)
                arrowforward.Draw(spriteBatch);
            spriteBatch.End();
        }

        private void Draw_Items()
        {
            for (int i = 0; i < 4; i++)
                if (party.Members[party.MemberOnTurn].Items[i] != null)
                {
                    int x = SELECTIONBOX_COLUMN2_X;
                    int y = SELECTIONBOX_ROW1_Y;
                    switch (i)
                    {
                        case 0:
                            x = SELECTIONBOX_COLUMN2_X;
                            y = SELECTIONBOX_ROW1_Y;
                            break;

                        case 1:
                            x = SELECTIONBOX_COLUMN1_X;
                            y = SELECTIONBOX_ROW2_Y;
                            break;

                        case 2:
                            x = SELECTIONBOX_COLUMN3_X;
                            y = SELECTIONBOX_ROW2_Y;
                            break;

                        case 3:
                            x = SELECTIONBOX_COLUMN2_X;
                            y = SELECTIONBOX_ROW3_Y;
                            break;
                    }
                    party.Members[party.MemberOnTurn].Items[i].DrawAt(spriteBatch, tempMenuPosition + new Vector2(x, y), i == selectedItemNumber);
                }
        }

        private void Draw_Items_SwapMenu()
        {
            for (int i = 0; i < 4; i++)
                if (party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].Items[i] != null)
                {
                    int x = SELECTIONBOX_COLUMN2_X;
                    int y = SELECTIONBOX_ROW1_Y;
                    switch (i)
                    {
                        case 0:
                            x = SELECTIONBOX_COLUMN2_X;
                            y = SELECTIONBOX_ROW1_Y;
                            break;

                        case 1:
                            x = SELECTIONBOX_COLUMN1_X;
                            y = SELECTIONBOX_ROW2_Y;
                            break;

                        case 2:
                            x = SELECTIONBOX_COLUMN3_X;
                            y = SELECTIONBOX_ROW2_Y;
                            break;

                        case 3:
                            x = SELECTIONBOX_COLUMN2_X;
                            y = SELECTIONBOX_ROW3_Y;
                            break;
                    }
                    party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].Items[i].DrawAt(spriteBatch, tempMenuPosition + new Vector2(x, y), i == selectedSwapItemNumber);
                }
        }

        private void Draw_Items_EquipWeaponMenu()
        {
            for (int i = 0; i < 4; i++)
                if (weapons[i] != -1)
                {
                    int x = SELECTIONBOX_COLUMN2_X;
                    int y = SELECTIONBOX_ROW1_Y;
                    switch (i)
                    {
                        case 0:
                            x = SELECTIONBOX_COLUMN2_X;
                            y = SELECTIONBOX_ROW1_Y;
                            break;

                        case 1:
                            x = SELECTIONBOX_COLUMN1_X;
                            y = SELECTIONBOX_ROW2_Y;
                            break;

                        case 2:
                            x = SELECTIONBOX_COLUMN3_X;
                            y = SELECTIONBOX_ROW2_Y;
                            break;

                        case 3:
                            x = SELECTIONBOX_COLUMN2_X;
                            y = SELECTIONBOX_ROW3_Y;
                            break;
                    }
                    party.Members[party.MemberOnTurn].Items[weapons[i]].DrawAt(spriteBatch, tempMenuPosition + new Vector2(x, y), i == selectedWeaponNumber);
                }
                else if (i == 3)
                    nothing.DrawAt(spriteBatch, tempMenuPosition + new Vector2(SELECTIONBOX_COLUMN2_X, SELECTIONBOX_ROW3_Y), i == selectedWeaponNumber);
        }

        private void Draw_Items_EquipRingMenu()
        {
            for (int i = 0; i < 4; i++)
                if (rings[i] != -1)
                {
                    int x = SELECTIONBOX_COLUMN2_X;
                    int y = SELECTIONBOX_ROW1_Y;
                    switch (i)
                    {
                        case 0:
                            x = SELECTIONBOX_COLUMN2_X;
                            y = SELECTIONBOX_ROW1_Y;
                            break;

                        case 1:
                            x = SELECTIONBOX_COLUMN1_X;
                            y = SELECTIONBOX_ROW2_Y;
                            break;

                        case 2:
                            x = SELECTIONBOX_COLUMN3_X;
                            y = SELECTIONBOX_ROW2_Y;
                            break;

                        case 3:
                            x = SELECTIONBOX_COLUMN2_X;
                            y = SELECTIONBOX_ROW3_Y;
                            break;
                    }
                    party.Members[party.MemberOnTurn].Items[rings[i]].DrawAt(spriteBatch, tempMenuPosition + new Vector2(x, y), i == selectedRingNumber);
                }
                else if (i == 3)
                    nothing.DrawAt(spriteBatch, tempMenuPosition + new Vector2(SELECTIONBOX_COLUMN2_X, SELECTIONBOX_ROW3_Y), i == selectedRingNumber);
        }

        private void Draw_Magic()
        {
            for (int i = 0; i < 4; i++)
                if (party.Members[party.MemberOnTurn].MagicSpells[i] != null)
                {
                    int x = SELECTIONBOX_COLUMN2_X;
                    int y = SELECTIONBOX_ROW1_Y;
                    switch (i)
                    {
                        case 0:
                            x = SELECTIONBOX_COLUMN2_X;
                            y = SELECTIONBOX_ROW1_Y;
                            break;

                        case 1:
                            x = SELECTIONBOX_COLUMN1_X;
                            y = SELECTIONBOX_ROW2_Y;
                            break;

                        case 2:
                            x = SELECTIONBOX_COLUMN3_X;
                            y = SELECTIONBOX_ROW2_Y;
                            break;

                        case 3:
                            x = SELECTIONBOX_COLUMN2_X;
                            y = SELECTIONBOX_ROW3_Y;
                            break;
                    }
                    party.Members[party.MemberOnTurn].MagicSpells[i].DrawAt(spriteBatch, tempMenuPosition + new Vector2(x, y), i == selectedSpellNumber);
                }
        }

        private void Draw_TextMessage()
        {
            Draw_TextMessage(textMessageBox.Position);
        }

        private void Draw_TextMessage(Vector2 position)
        {
            italicFont.Position = position + new Vector2(24, 17);
            for (int i = 0; i < positionInTextMessage.Row; i++)
                italicFont.Println(spriteBatch, textMessage[i], true);
            italicFont.Println(spriteBatch, textMessage[positionInTextMessage.Row].Substring(0, positionInTextMessage.Column + 1), true);
        }

        private void Draw_EquipSpecsBox()
        {
            equipSpecsBox.Draw(spriteBatch);
            smallFont.Position = equipSpecsBox.Position + new Vector2(20, 15);
            smallFont.Println(spriteBatch, "ATT" + RightAlign(itoa(party.Members[party.MemberOnTurn].AttackPoints), 5));
            smallFont.Println(spriteBatch, "");
            smallFont.Println(spriteBatch, "DEF" + RightAlign(itoa(party.Members[party.MemberOnTurn].DefensePoints), 5));
            smallFont.Println(spriteBatch, "");
            smallFont.Println(spriteBatch, "AGI" + RightAlign(itoa(party.Members[party.MemberOnTurn].Agility), 5));
            smallFont.Println(spriteBatch, "");
            smallFont.Println(spriteBatch, "MOV" + RightAlign(itoa(party.Members[party.MemberOnTurn].MovePoints), 5));
        }

        private void Draw_TitleScreenMode(GameTime gameTime)
        {
            spriteBatch.Begin();
            TitleScreen.Draw(spriteBatch);
            spriteBatch.End();
        }

        private string RightAlign(string text, int len)
        {
            while (text.Length < len)
                text = " " + text;

            return text;
        }

        private string itoa(int integer)
        {
            if (integer > 99)
                return "??";
            else
                return integer.ToString();
        }

        private void Blink(GameTime gameTime)
        {
            double elapsedTime = gameTime.TotalGameTime.TotalMilliseconds - gameTimeAtLastBlink;

            if (!blinkDelayReached && elapsedTime >= 1000 / 60 * BLINK_DELAY)
            {
                blinkDelayReached = true;

                if (map != null)
                    map.BlinkStatus = !map.BlinkStatus;

                if (GameMode == GameModes.SelectionBarMode || GameMode == GameModes.BattleMenuMode || GameMode == GameModes.BattleMenuMoveInMode || GameMode == GameModes.BattleMenuMoveOutMode || GameMode == GameModes.TransitionInSelectionBarMode || GameMode == GameModes.AttackMenuMode || GameMode == GameModes.TransitionInAttackMenuMode || GameMode == GameModes.SelectionBarTransitionOutAttackMenuMode || GameMode == GameModes.GeneralMenuMode || GameMode == GameModes.GeneralMenuMoveInMode || GameMode == GameModes.GeneralMenuMoveOutMode || GameMode == GameModes.ItemMenuMode || GameMode == GameModes.ItemMenuMoveInMode || GameMode == GameModes.ItemMenuMoveOutMode || GameMode == GameModes.ItemMagicSelectionMenuMode || GameMode == GameModes.DisplayTextMessageMode || GameMode == GameModes.GiveItemMode || GameMode == GameModes.SelectionBarTransitionInGiveItemMode || GameMode == GameModes.SelectionBarTransitionOutGiveItemMode || GameMode == GameModes.SwapItemMode || GameMode == GameModes.SwapItemMoveInMode || GameMode == GameModes.SwapItemMoveOutMode || GameMode == GameModes.EquipWeaponMode || GameMode == GameModes.EquipWeaponMoveInMode || GameMode == GameModes.EquipWeaponMoveOutMode || GameMode == GameModes.EquipRingMode || GameMode == GameModes.EquipRingMoveInMode || GameMode == GameModes.EquipRingMoveOutMode || GameMode == GameModes.PlayerBattleMode || GameMode == GameModes.PlayerBattleDisplayTextMessageMode || GameMode == GameModes.MagicLevelSelectionMode || GameMode == GameModes.HealMenuMode || GameMode == GameModes.MemberMenuMode)
                {
                    party.Members[party.MemberOnTurn].Visible = false;
                    Arrow.Visible = false;
                }

                if (GameMode == GameModes.EnemyBattleMode || GameMode == GameModes.EnemyBattleDisplayTextMessageMode || GameMode == GameModes.EnemyAttackMode || GameMode == GameModes.TransitionInEnemyAttackMode)
                {
                    enemies.Members[enemies.MemberOnTurn].Visible = false;
                    Arrow.Visible = false;
                }
            }

            if (elapsedTime >= 1000 / 60 * 2 * BLINK_DELAY)
            {
                gameTimeAtLastBlink = gameTime.TotalGameTime.TotalMilliseconds;
                blinkDelayReached = false;

                if (map != null)
                    map.BlinkStatus = !map.BlinkStatus;

                if (GameMode == GameModes.SelectionBarMode || GameMode == GameModes.BattleMenuMode || GameMode == GameModes.BattleMenuMoveInMode || GameMode == GameModes.BattleMenuMoveOutMode || GameMode == GameModes.TransitionInSelectionBarMode || GameMode == GameModes.AttackMenuMode || GameMode == GameModes.TransitionInAttackMenuMode || GameMode == GameModes.SelectionBarTransitionOutAttackMenuMode || GameMode == GameModes.GeneralMenuMode || GameMode == GameModes.GeneralMenuMoveInMode || GameMode == GameModes.GeneralMenuMoveOutMode || GameMode == GameModes.ItemMenuMode || GameMode == GameModes.ItemMenuMoveInMode || GameMode == GameModes.ItemMenuMoveOutMode || GameMode == GameModes.ItemMagicSelectionMenuMode || GameMode == GameModes.DisplayTextMessageMode || GameMode == GameModes.GiveItemMode || GameMode == GameModes.SelectionBarTransitionInGiveItemMode || GameMode == GameModes.SelectionBarTransitionOutGiveItemMode || GameMode == GameModes.SwapItemMode || GameMode == GameModes.SwapItemMoveInMode || GameMode == GameModes.SwapItemMoveOutMode || GameMode == GameModes.EquipWeaponMode || GameMode == GameModes.EquipWeaponMoveInMode || GameMode == GameModes.EquipWeaponMoveOutMode || GameMode == GameModes.EquipRingMode || GameMode == GameModes.EquipRingMoveInMode || GameMode == GameModes.EquipRingMoveOutMode || GameMode == GameModes.PlayerBattleMode || GameMode == GameModes.PlayerBattleDisplayTextMessageMode || GameMode == GameModes.MagicLevelSelectionMode || GameMode == GameModes.HealMenuMode || GameMode == GameModes.MemberMenuMode)
                {
                    party.Members[party.MemberOnTurn].Visible = true;
                    Arrow.Visible = true;
                }

                if (GameMode == GameModes.EnemyBattleMode || GameMode == GameModes.EnemyBattleDisplayTextMessageMode || GameMode == GameModes.EnemyAttackMode || GameMode == GameModes.TransitionInEnemyAttackMode)
                {
                    enemies.Members[enemies.MemberOnTurn].Visible = true;
                    Arrow.Visible = true;
                }

                for (int i = 0; i < party.NumPartyMembers; i++)
                {
                    if (party.Members[i].Alive)
                    {
                        if ((int)party.Members[i].PositionStatus % 2 == 1)
                            party.Members[i].PositionStatus--;
                        else
                            party.Members[i].PositionStatus++;
                    }
                }

                if (enemies != null)
                {
                    for (int i = 0; i < enemies.NumPartyMembers; i++)
                    {
                        if (enemies.Members[i].Alive)
                        {
                            if ((int)enemies.Members[i].PositionStatus % 2 == 1)
                                enemies.Members[i].PositionStatus--;
                            else
                                enemies.Members[i].PositionStatus++;
                        }
                    }
                }

                if ((int)battleMenu.CurrentState % 2 == 1)
                    battleMenu.CurrentState--;
                else
                    battleMenu.CurrentState++;

                if ((int)generalMenu.CurrentState % 2 == 1)
                    generalMenu.CurrentState--;
                else
                    generalMenu.CurrentState++;

                if ((int)itemMenu.CurrentState % 2 == 1)
                    itemMenu.CurrentState--;
                else
                    itemMenu.CurrentState++;

                if ((int)Item.CurrentState % 2 == 1)
                    Item.CurrentState--;
                else
                    Item.CurrentState++;

                if ((int)yesNoButtons.CurrentState % 2 == 1)
                    yesNoButtons.CurrentState--;
                else
                    yesNoButtons.CurrentState++;

                if (GameMode == GameModes.ItemMagicSelectionMenuMode)
                {
                    if ((int)Spell.CurrentState % 2 == 1)
                        Spell.CurrentState--;
                    else
                        Spell.CurrentState++;
                }

                if (Redbar.CurrentState % 2 == 1)
                    Redbar.CurrentState--;
                else
                    Redbar.CurrentState++;
            }
        }

        private void UpdateVariables()
        {
            switch (GameMode)
            {
                case GameModes.SelectionBarMode:
                    UpdateVariables_SelectionBarMode();
                    break;

                case GameModes.SelectionBarTransitionMode:
                    UpdateVariables_SelectionBarTransitionMode();
                    break;

                case GameModes.PlayerMoveTransitionMode:
                    UpdateVariables_PlayerMoveTransitionMode();
                    break;

                case GameModes.PlayerMovingMode:
                    UpdateVariables_PlayerMovingMode();
                    break;

                case GameModes.PlayerMovingInSelectionBarTransitionMode:
                    UpdateVariables_PlayerMovingInSelectionBarTransitionMode();
                    break;

                case GameModes.TransitionInPlayerMoveTransitionMode:
                    UpdateVariables_TransitionInPlayerMoveTransitionMode();
                    break;

                case GameModes.TransitionInSelectionBarMode:
                    UpdateVariables_TransitionInSelectionBarMode();
                    break;

                case GameModes.AttackMenuMode:
                    UpdateVariables_AttackMenuMode();
                    break;

                case GameModes.TransitionInAttackMenuMode:
                    UpdateVariables_TransitionInAttackMenuMode();
                    break;

                case GameModes.SelectionBarTransitionOutAttackMenuMode:
                    UpdateVariables_SelectionBarTransitionOutAttackMenuMode();
                    break;

                case GameModes.GeneralMenuMoveOutMode:
                    UpdateVariables_GeneralMenuMoveOutMode();
                    break;

                case GameModes.GeneralMenuMoveInMode:
                    UpdateVariables_GeneralMenuMoveInMode();
                    break;

                case GameModes.BattleMenuMoveOutMode:
                    UpdateVariables_BattleMenuMoveOutMode();
                    break;

                case GameModes.BattleMenuMoveInMode:
                    UpdateVariables_BattleMenuMoveInMode();
                    break;

                case GameModes.ItemMenuMoveOutMode:
                    UpdateVariables_ItemMenuMoveOutMode();
                    break;

                case GameModes.ItemMenuMoveInMode:
                    UpdateVariables_ItemMenuMoveInMode();
                    break;

                case GameModes.ItemMagicSelectionMenuMoveInMode:
                    UpdateVariables_ItemMagicSelectionMenuMoveInMode();
                    break;

                case GameModes.ItemMagicSelectionMenuMoveOutMode:
                    UpdateVariables_ItemMagicSelectionMenuMoveOutMode();
                    break;

                case GameModes.DisplayTextMessageMode:
                    UpdateVariables_DisplayTextMessageMode();
                    break;

                case GameModes.DisplayTextMessageMoveOutMode:
                    UpdateVariables_DisplayTextMessageMoveOutMode();
                    break;

                case GameModes.GiveItemMode:
                    UpdateVariables_GiveItemMode();
                    break;

                case GameModes.SelectionBarTransitionInGiveItemMode:
                    UpdateVariables_SelectionBarTransitionInGiveItemMode();
                    break;

                case GameModes.SelectionBarTransitionOutGiveItemMode:
                    UpdateVariables_SelectionBarTransitionOutGiveItemMode();
                    break;

                case GameModes.SwapItemMoveInMode:
                    UpdateVariables_SwapItemMoveInMode();
                    break;

                case GameModes.SwapItemMoveOutMode:
                    UpdateVariables_SwapItemMoveOutMode();
                    break;

                case GameModes.EquipWeaponMoveInMode:
                    UpdateVariables_EquipWeaponMoveInMode();
                    break;

                case GameModes.EquipWeaponMoveOutMode:
                    UpdateVariables_EquipWeaponMoveOutMode();
                    break;

                case GameModes.EquipRingMoveInMode:
                    UpdateVariables_EquipRingMoveInMode();
                    break;

                case GameModes.EquipRingMoveOutMode:
                    UpdateVariables_EquipRingMoveOutMode();
                    break;

                case GameModes.PlayerBattleMode:
                    UpdateVariables_PlayerBattleMode();
                    break;

                case GameModes.DisplayDefeatedCharactersMode:
                    UpdateVariables_DisplayDefeatedCharactersMode();
                    break;

                case GameModes.NextCharacterAfterSleepMode:
                    UpdateVariables_NextCharacterAfterSleepMode();
                    break;

                case GameModes.PlayerBattleDisplayTextMessageMode:
                    UpdateVariables_PlayerBattleDisplayTextMessageMode();
                    break;

                case GameModes.BattleDisplayTextMessageMoveOutMode:
                    UpdateVariables_BattleDisplayTextMessageMoveOutMode();
                    break;

                case GameModes.EnemyMoveTransitionMode:
                    UpdateVariables_EnemyMoveTransitionMode();
                    break;

                case GameModes.TransitionInEnemyMoveTransitionMode:
                    UpdateVariables_TransitionInEnemyMoveTransitionMode();
                    break;

                case GameModes.EnemyMoveMode:
                    UpdateVariables_EnemyMoveMode();
                    break;

                case GameModes.EnemyMovingMode:
                    UpdateVariables_EnemyMovingMode();
                    break;

                case GameModes.EnemyBattleMode:
                    UpdateVariables_EnemyBattleMode();
                    break;

                case GameModes.EnemyBattleDisplayTextMessageMode:
                    UpdateVariables_EnemyBattleDisplayTextMessageMode();
                    break;

                case GameModes.EnemyAttackMode:
                    UpdateVariables_EnemyAttackMode();
                    break;

                case GameModes.TransitionInEnemyAttackMode:
                    UpdateVariables_TransitionInEnemyAttackMode();
                    break;

                case GameModes.HealMenuMode:
                    UpdateVariables_HealMenuMode();
                    break;

                case GameModes.TransitionInHealMenuMode:
                    UpdateVariables_TransitionInHealMenuMode();
                    break;

                case GameModes.EndTurnOfCharacterAndNextCharacterMode:
                    UpdateVariables_EndTurnOfCharacterAndNextCharacterMode();
                    break;

                case GameModes.PlayerAutoMoveMode:
                    UpdateVariables_PlayerAutoMoveMode();
                    break;

                case GameModes.PlayerAutoAttackMode:
                    UpdateVariables_PlayerAutoAttackMode();
                    break;

                case GameModes.BattleFieldFadeOutMode:
                    UpdateVariables_BattleFieldFadeOutMode();
                    break;

                case GameModes.BattleFieldFadeInMode:
                    UpdateVariables_BattleFieldFadeInMode();
                    break;

                case GameModes.AfterTitleScreenMode:
                    UpdateVariables_AfterTitleScreenMode();
                    break;
            }
        }

        private void UpdateVariables_SelectionBarMode()
        {
            if (map.Offset.X != 0 || map.Offset.Y != 0)
            {
                tempSelectionBarPosition.X = oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX;
                tempSelectionBarPosition.Y = oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY;

                GameMode = GameModes.TransitionInSelectionBarMode;
            }

            if (oldSelectionBarPosition != selectionBar.Position)
            {
                tempSelectionBarPosition.X = oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX;
                tempSelectionBarPosition.Y = oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY;

                if (oldSelectionBarPosition.X < selectionBar.Position.X)
                    oldSelectionBarPosition.X++;
                else if (oldSelectionBarPosition.X > selectionBar.Position.X)
                    oldSelectionBarPosition.X--;

                if (oldSelectionBarPosition.Y < selectionBar.Position.Y)
                    oldSelectionBarPosition.Y++;
                else if (oldSelectionBarPosition.Y > selectionBar.Position.Y)
                    oldSelectionBarPosition.Y--;

                GameMode = GameModes.TransitionInSelectionBarMode;
            }
        }

        private void UpdateVariables_SelectionBarTransitionMode()
        {
            if (party.Members[party.MemberOnTurn].Position != backUpPosition)
            {
                EnterPlayerMovingInSelectionBarTransitionMode();
                switch (map.bestPath[currentStepNumber])
                {
                    case Map.Directions.Up:
                        party.Members[party.MemberOnTurn].Position -= map.Size.X;
                        party.Members[party.MemberOnTurn].LookUp();
                        break;

                    case Map.Directions.Left:
                        party.Members[party.MemberOnTurn].Position -= 1;
                        party.Members[party.MemberOnTurn].LookLeft();
                        break;

                    case Map.Directions.Right:
                        party.Members[party.MemberOnTurn].Position += 1;
                        party.Members[party.MemberOnTurn].LookRight();
                        break;

                    case Map.Directions.Down:
                        party.Members[party.MemberOnTurn].Position += map.Size.X;
                        party.Members[party.MemberOnTurn].LookDown();
                        break;
                }
                currentStepNumber++;
            }
            else
            {
                party.Members[party.MemberOnTurn].LookDown();

                if (oldMapPosition != map.Position)
                {
                    if (oldMapPosition.X < map.Position.X)
                        oldMapPosition.X++;
                    else if (oldMapPosition.X > map.Position.X)
                        oldMapPosition.X--;

                    if (oldMapPosition.Y < map.Position.Y)
                        oldMapPosition.Y++;
                    else if (oldMapPosition.Y > map.Position.Y)
                        oldMapPosition.Y--;
                }
                else
                    GameMode = GameModes.SelectionBarMode;
            }
        }

        private void UpdateVariables_PlayerMoveTransitionMode()
        {
            if (oldMapPosition == map.Position && oldSelectionBarPosition == selectionBar.Position)
            {
                if (party.Members[party.MemberOnTurn].Confused > 0)
                    EnterPlayerAutoMoveMode();
                else
                    EnterPlayerMoveMode();
            }

            if (oldMapPosition != map.Position)
            {
                if (oldMapPosition.X < map.Position.X)
                {
                    oldMapPosition.X++;
                    map.Offset.X = Map.TILESIZEX;
                }
                else if (oldMapPosition.X > map.Position.X)
                {
                    oldMapPosition.X--;
                    map.Offset.X = -Map.TILESIZEX;
                }

                if (oldMapPosition.Y < map.Position.Y)
                {
                    oldMapPosition.Y++;
                    map.Offset.Y = Map.TILESIZEY;
                }
                else if (oldMapPosition.Y > map.Position.Y)
                {
                    oldMapPosition.Y--;
                    map.Offset.Y = -Map.TILESIZEY;
                }

                tempSelectionBarPosition.X = oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX;
                tempSelectionBarPosition.Y = oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY;

                GameMode = GameModes.TransitionInPlayerMoveTransitionMode;
            }

            if (oldSelectionBarPosition != selectionBar.Position)
            {
                tempSelectionBarPosition.X = oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX;
                tempSelectionBarPosition.Y = oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY;

                if (oldSelectionBarPosition.X < selectionBar.Position.X)
                    oldSelectionBarPosition.X++;
                else if (oldSelectionBarPosition.X > selectionBar.Position.X)
                    oldSelectionBarPosition.X--;

                if (oldSelectionBarPosition.Y < selectionBar.Position.Y)
                    oldSelectionBarPosition.Y++;
                else if (oldSelectionBarPosition.Y > selectionBar.Position.Y)
                    oldSelectionBarPosition.Y--;

                GameMode = GameModes.TransitionInPlayerMoveTransitionMode;
            }
        }

        private void UpdateVariables_PlayerMovingMode()
        {
            if (map.Offset.X != 0 || map.Offset.Y != 0)
            {
                if (map.Offset.X < 0)
                    map.Offset.X += MOVEX;
                else if (map.Offset.X > 0)
                    map.Offset.X -= MOVEX;

                if (map.Offset.Y < 0)
                    map.Offset.Y += MOVEY;
                else if (map.Offset.Y > 0)
                    map.Offset.Y -= MOVEY;
            }
            else if (delayPosition.X == map.CalcPosition(party.Members[party.MemberOnTurn].Position).X * Map.TILESIZEX
                && delayPosition.Y == map.CalcPosition(party.Members[party.MemberOnTurn].Position).Y * Map.TILESIZEY)
            {
                if (party.Members[party.MemberOnTurn].Confused > 0)
                    GameMode = GameModes.PlayerAutoMoveMode;
                else
                    GameMode = GameModes.PlayerMoveMode;
            }
            else
            {
                if (delayPosition.X < map.CalcPosition(party.Members[party.MemberOnTurn].Position).X * Map.TILESIZEX)
                    delayPosition.X += MOVEX;
                else if (delayPosition.X > map.CalcPosition(party.Members[party.MemberOnTurn].Position).X * Map.TILESIZEX)
                    delayPosition.X -= MOVEX;

                if (delayPosition.Y < map.CalcPosition(party.Members[party.MemberOnTurn].Position).Y * Map.TILESIZEY)
                    delayPosition.Y += MOVEY;
                else if (delayPosition.Y > map.CalcPosition(party.Members[party.MemberOnTurn].Position).Y * Map.TILESIZEY)
                    delayPosition.Y -= MOVEY;
            }
        }

        private void UpdateVariables_PlayerMovingInSelectionBarTransitionMode()
        {
            if (delayPosition.X == map.CalcPosition(party.Members[party.MemberOnTurn].Position).X * Map.TILESIZEX
                && delayPosition.Y == map.CalcPosition(party.Members[party.MemberOnTurn].Position).Y * Map.TILESIZEY)
                GameMode = GameModes.SelectionBarTransitionMode;
            else
            {
                if (delayPosition.X < map.CalcPosition(party.Members[party.MemberOnTurn].Position).X * Map.TILESIZEX)
                    delayPosition.X += 2 * MOVEX;
                else if (delayPosition.X > map.CalcPosition(party.Members[party.MemberOnTurn].Position).X * Map.TILESIZEX)
                    delayPosition.X -= 2 * MOVEX;

                if (delayPosition.Y < map.CalcPosition(party.Members[party.MemberOnTurn].Position).Y * Map.TILESIZEY)
                    delayPosition.Y += 2 * MOVEY;
                else if (delayPosition.Y > map.CalcPosition(party.Members[party.MemberOnTurn].Position).Y * Map.TILESIZEY)
                    delayPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_TransitionInPlayerMoveTransitionMode()
        {
            bool mapOkay = false;

            if (map.Offset.X == 0 && map.Offset.Y == 0)
                mapOkay = true;
            else
            {
                if (map.Offset.X < 0)
                    map.Offset.X += 2 * MOVEX;
                else if (map.Offset.X > 0)
                    map.Offset.X -= 2 * MOVEX;

                if (map.Offset.Y < 0)
                    map.Offset.Y += 2 * MOVEY;
                else if (map.Offset.Y > 0)
                    map.Offset.Y -= 2 * MOVEY;
            }

            if (tempSelectionBarPosition.X != oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX
                || tempSelectionBarPosition.Y != oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
            {
                if (tempSelectionBarPosition.X < oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X += 2 * MOVEX;
                else if (tempSelectionBarPosition.X > oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X -= 2 * MOVEX;

                if (tempSelectionBarPosition.Y < oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y += 2 * MOVEY;
                else if (tempSelectionBarPosition.Y > oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y -= 2 * MOVEY;
            }
            else
                if (mapOkay == true)
                    GameMode = GameModes.PlayerMoveTransitionMode;
        }

        private void UpdateVariables_TransitionInSelectionBarMode()
        {
            bool mapOkay = false;

            if (map.Offset.X != 0 || map.Offset.Y != 0)
            {
                if (map.Offset.X < 0)
                    map.Offset.X += 2 * MOVEX;
                else if (map.Offset.X > 0)
                    map.Offset.X -= 2 * MOVEX;

                if (map.Offset.Y < 0)
                    map.Offset.Y += 2 * MOVEY;
                else if (map.Offset.Y > 0)
                    map.Offset.Y -= 2 * MOVEY;
            }
            else
                mapOkay = true;

            if (tempSelectionBarPosition.X != oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX
                || tempSelectionBarPosition.Y != oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
            {
                if (tempSelectionBarPosition.X < oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X += 2 * MOVEX;
                else if (tempSelectionBarPosition.X > oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X -= 2 * MOVEX;

                if (tempSelectionBarPosition.Y < oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y += 2 * MOVEY;
                else if (tempSelectionBarPosition.Y > oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y -= 2 * MOVEY;
            }
            else
                if (mapOkay)
                    GameMode = GameModes.SelectionBarMode;
        }

        private void UpdateVariables_AttackMenuMode()
        {
            if (oldMapPosition != map.Position)
            {
                map.Offset.X = -Map.TILESIZEX * (oldMapPosition.X - map.Position.X);
                map.Offset.Y = -Map.TILESIZEY * (oldMapPosition.Y - map.Position.Y);

                tempSelectionBarPosition.X = oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX;
                tempSelectionBarPosition.Y = oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY;

                returnGameMode = GameModes.AttackMenuMode;
                GameMode = GameModes.TransitionInAttackMenuMode;
            }

            if (oldSelectionBarPosition != selectionBar.Position)
            {
                tempSelectionBarPosition.X = oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX;
                tempSelectionBarPosition.Y = oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY;

                if (oldSelectionBarPosition.X < selectionBar.Position.X)
                    oldSelectionBarPosition.X++;
                else if (oldSelectionBarPosition.X > selectionBar.Position.X)
                    oldSelectionBarPosition.X--;

                if (oldSelectionBarPosition.Y < selectionBar.Position.Y)
                    oldSelectionBarPosition.Y++;
                else if (oldSelectionBarPosition.Y > selectionBar.Position.Y)
                    oldSelectionBarPosition.Y--;

                returnGameMode = GameModes.AttackMenuMode;
                GameMode = GameModes.TransitionInAttackMenuMode;
            }
        }

        private void UpdateVariables_TransitionInAttackMenuMode()
        {
            bool mapOkay = false;

            if (map.Offset.X != 0 || map.Offset.Y != 0)
            {
                if (map.Offset.X < 0)
                    map.Offset.X += 2 * MOVEX;
                else if (map.Offset.X > 0)
                    map.Offset.X -= 2 * MOVEX;

                if (map.Offset.Y < 0)
                    map.Offset.Y += 2 * MOVEY;
                else if (map.Offset.Y > 0)
                    map.Offset.Y -= 2 * MOVEY;
            }
            else
            {
                oldMapPosition = map.Position;
                mapOkay = true;
            }

            if (tempSelectionBarPosition.X != oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX
                || tempSelectionBarPosition.Y != oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
            {
                if (tempSelectionBarPosition.X < oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X += 2 * MOVEX;
                else if (tempSelectionBarPosition.X > oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X -= 2 * MOVEX;

                if (tempSelectionBarPosition.Y < oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y += 2 * MOVEY;
                else if (tempSelectionBarPosition.Y > oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y -= 2 * MOVEY;
            }
            else if (mapOkay)
                ReturnToReturnGameMode();
        }

        private void UpdateVariables_SelectionBarTransitionOutAttackMenuMode()
        {
            if (tempSelectionBarPosition.X == oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX
                && tempSelectionBarPosition.Y == oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                EnterBattleMenuMode();
            else
            {
                if (tempSelectionBarPosition.X < oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X += 2 * MOVEX;
                else if (tempSelectionBarPosition.X > oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X -= 2 * MOVEX;

                if (tempSelectionBarPosition.Y < oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y += 2 * MOVEY;
                else if (tempSelectionBarPosition.Y > oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_GeneralMenuMoveOutMode()
        {
            if (tempMenuPosition == generalMenu.Position)
                GameMode = GameModes.SelectionBarMode;
            else
            {
                if (tempMenuPosition.X < generalMenu.Position.X)
                    tempMenuPosition.X += 2 * MOVEX;
                else if (tempMenuPosition.X > generalMenu.Position.X)
                    tempMenuPosition.X -= 2 * MOVEX;

                if (tempMenuPosition.Y < generalMenu.Position.Y)
                    tempMenuPosition.Y += 2 * MOVEY;
                else if (tempMenuPosition.Y > generalMenu.Position.Y)
                    tempMenuPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_GeneralMenuMoveInMode()
        {
            if (tempMenuPosition == generalMenu.Position)
                EnterGeneralMenuMode();
            else
            {
                if (tempMenuPosition.X < generalMenu.Position.X)
                    tempMenuPosition.X += 2 * MOVEX;
                else if (tempMenuPosition.X > generalMenu.Position.X)
                    tempMenuPosition.X -= 2 * MOVEX;

                if (tempMenuPosition.Y < generalMenu.Position.Y)
                    tempMenuPosition.Y += 2 * MOVEY;
                else if (tempMenuPosition.Y > generalMenu.Position.Y)
                    tempMenuPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_BattleMenuMoveOutMode()
        {
            if (tempMenuPosition == battleMenu.Position)
                LeaveBattleMenuMoveOutMode();
            else
            {
                if (tempMenuPosition.X < battleMenu.Position.X)
                    tempMenuPosition.X += 2 * MOVEX;
                else if (tempMenuPosition.X > battleMenu.Position.X)
                    tempMenuPosition.X -= 2 * MOVEX;

                if (tempMenuPosition.Y < battleMenu.Position.Y)
                    tempMenuPosition.Y += 2 * MOVEY;
                else if (tempMenuPosition.Y > battleMenu.Position.Y)
                    tempMenuPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_BattleMenuMoveInMode()
        {
            if (tempMenuPosition == battleMenu.Position)
                EnterBattleMenuMode();
            else
            {
                if (tempMenuPosition.X < battleMenu.Position.X)
                    tempMenuPosition.X += 2 * MOVEX;
                else if (tempMenuPosition.X > battleMenu.Position.X)
                    tempMenuPosition.X -= 2 * MOVEX;

                if (tempMenuPosition.Y < battleMenu.Position.Y)
                    tempMenuPosition.Y += 2 * MOVEY;
                else if (tempMenuPosition.Y > battleMenu.Position.Y)
                    tempMenuPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_ItemMenuMoveOutMode()
        {
            if (tempMenuPosition == itemMenu.Position)
                EnterBattleMenuMode();
            else
            {
                if (tempMenuPosition.X < itemMenu.Position.X)
                    tempMenuPosition.X += 2 * MOVEX;
                else if (tempMenuPosition.X > itemMenu.Position.X)
                    tempMenuPosition.X -= 2 * MOVEX;

                if (tempMenuPosition.Y < itemMenu.Position.Y)
                    tempMenuPosition.Y += 2 * MOVEY;
                else if (tempMenuPosition.Y > itemMenu.Position.Y)
                    tempMenuPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_ItemMenuMoveInMode()
        {
            if (tempMenuPosition == itemMenu.Position)
                GameMode = GameModes.ItemMenuMode;
            else
            {
                if (tempMenuPosition.X < itemMenu.Position.X)
                    tempMenuPosition.X += 2 * MOVEX;
                else if (tempMenuPosition.X > itemMenu.Position.X)
                    tempMenuPosition.X -= 2 * MOVEX;

                if (tempMenuPosition.Y < itemMenu.Position.Y)
                    tempMenuPosition.Y += 2 * MOVEY;
                else if (tempMenuPosition.Y > itemMenu.Position.Y)
                    tempMenuPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_ItemMagicSelectionMenuMoveInMode()
        {
            if (tempMenuPosition == itemMagicSelectionMenu.Position)
                EnterItemMagicSelectionMenuMode();
            else
            {
                if (tempMenuPosition.X < itemMagicSelectionMenu.Position.X)
                    tempMenuPosition.X += 2 * MOVEX;
                else if (tempMenuPosition.X > itemMagicSelectionMenu.Position.X)
                    tempMenuPosition.X -= 2 * MOVEX;

                if (tempMenuPosition.Y < itemMagicSelectionMenu.Position.Y)
                    tempMenuPosition.Y += 2 * MOVEY;
                else if (tempMenuPosition.Y > itemMagicSelectionMenu.Position.Y)
                    tempMenuPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_ItemMagicSelectionMenuMoveOutMode()
        {
            if (tempMenuPosition == itemMagicSelectionMenu.Position)
            {
                if (battleMenu.CurrentState == BattleMenu.States.ItemSelected1 || battleMenu.CurrentState == BattleMenu.States.ItemSelected2)
                    EnterBattleMenuMode();
                    // EnterItemMenuMode();
                    // In Shining Force 2 the game enters battle menu mode at this stage. In this game items can only be used, not given, equipped or discarded.
                else if (battleMenu.CurrentState == BattleMenu.States.MagicSelected1 || battleMenu.CurrentState == BattleMenu.States.MagicSelected2)
                    EnterBattleMenuMode();
            }
            else
            {
                if (tempMenuPosition.X < itemMagicSelectionMenu.Position.X)
                    tempMenuPosition.X += 2 * MOVEX;
                else if (tempMenuPosition.X > itemMagicSelectionMenu.Position.X)
                    tempMenuPosition.X -= 2 * MOVEX;

                if (tempMenuPosition.Y < itemMagicSelectionMenu.Position.Y)
                    tempMenuPosition.Y += 2 * MOVEY;
                else if (tempMenuPosition.Y > itemMagicSelectionMenu.Position.Y)
                    tempMenuPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_DisplayTextMessageMode()
        {
            textMessageDelay++;

            if (positionInTextMessage.Column < textMessage[positionInTextMessage.Row].Length - 1)
            {
                if ((speed == 1 && textMessageDelay >= 6) || (speed == 2 && textMessageDelay >= 4) || (speed == 3 && textMessageDelay >= 2) || speed == 4)
                {
                    positionInTextMessage.Column++;
                    textMessageDelay = 0;
                }
            }
            else if (positionInTextMessage.Row < textMessageLength - 1)
            {
                if (positionInTextMessage.Row == 2)
                {
                    if (textMessageCounter != 0 || textMessageContinueFlag)
                    {
                        for (int i = 0; i < textMessageLength; i++)
                            textMessage[i] = textMessage[i + 1];
                        textMessageLength--;
                        positionInTextMessage.Column = 0;
                        textMessageContinueFlag = false;
                        textMessageCounter++;
                        textMessageCounter %= 3;
                    }
                }
                else
                {
                    positionInTextMessage.Row++;
                    positionInTextMessage.Column = 0;
                }
            }
            else if (!waitForKeyPress)
            {
                if (moveOut)
                    EnterDisplayTextMessageMoveOutMode();
                else
                    ReturnToReturnGameMode();
            }
        }

        private void UpdateVariables_DisplayTextMessageMoveOutMode()
        {
            if (tempTextMessageBoxPosition == textMessageBox.Position)
                ReturnToReturnGameMode();
            else
            {
                if (tempTextMessageBoxPosition.X < textMessageBox.Position.X)
                    tempTextMessageBoxPosition.X += 2 * MOVEX;
                else if (tempTextMessageBoxPosition.X > textMessageBox.Position.X)
                    tempTextMessageBoxPosition.X -= 2 * MOVEX;

                if (tempTextMessageBoxPosition.Y < textMessageBox.Position.Y)
                    tempTextMessageBoxPosition.Y += 2 * MOVEY;
                else if (tempTextMessageBoxPosition.Y > textMessageBox.Position.Y)
                    tempTextMessageBoxPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_GiveItemMode()
        {
            if (oldSelectionBarPosition != selectionBar.Position)
            {
                tempSelectionBarPosition.X = oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX;
                tempSelectionBarPosition.Y = oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY;

                if (oldSelectionBarPosition.X < selectionBar.Position.X)
                    oldSelectionBarPosition.X++;
                else if (oldSelectionBarPosition.X > selectionBar.Position.X)
                    oldSelectionBarPosition.X--;

                if (oldSelectionBarPosition.Y < selectionBar.Position.Y)
                    oldSelectionBarPosition.Y++;
                else if (oldSelectionBarPosition.Y > selectionBar.Position.Y)
                    oldSelectionBarPosition.Y--;

                GameMode = GameModes.SelectionBarTransitionInGiveItemMode;
            }
        }

        private void UpdateVariables_SelectionBarTransitionInGiveItemMode()
        {
            if (tempSelectionBarPosition.X == oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX
                && tempSelectionBarPosition.Y == oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                GameMode = GameModes.GiveItemMode;
            else
            {
                if (tempSelectionBarPosition.X < oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X += 2 * MOVEX;
                else if (tempSelectionBarPosition.X > oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X -= 2 * MOVEX;

                if (tempSelectionBarPosition.Y < oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y += 2 * MOVEY;
                else if (tempSelectionBarPosition.Y > oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_SelectionBarTransitionOutGiveItemMode()
        {
            if (tempSelectionBarPosition.X == oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX
                && tempSelectionBarPosition.Y == oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                EnterItemMagicSelectionMenuMode();
            else
            {
                if (tempSelectionBarPosition.X < oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X += 2 * MOVEX;
                else if (tempSelectionBarPosition.X > oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X -= 2 * MOVEX;

                if (tempSelectionBarPosition.Y < oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y += 2 * MOVEY;
                else if (tempSelectionBarPosition.Y > oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_SwapItemMoveInMode()
        {
            if (tempMenuPosition == swapMenu.Position)
                EnterSwapItemMode();
            else
            {
                if (tempMenuPosition.X < swapMenu.Position.X)
                    tempMenuPosition.X += 2 * MOVEX;
                else if (tempMenuPosition.X > swapMenu.Position.X)
                    tempMenuPosition.X -= 2 * MOVEX;

                if (tempMenuPosition.Y < swapMenu.Position.Y)
                    tempMenuPosition.Y += 2 * MOVEY;
                else if (tempMenuPosition.Y > swapMenu.Position.Y)
                    tempMenuPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_SwapItemMoveOutMode()
        {
            if (tempMenuPosition == swapMenu.Position)
            {
                selectionBar.Position = map.CalcPosition(party.Members[party.MemberOnTurn].Position);
                party.Members[party.MemberOnTurn].LookDown();
                EnterEndTurnOfCharacterAndNextCharacterMode();
            }
            else
            {
                if (tempMenuPosition.X < swapMenu.Position.X)
                    tempMenuPosition.X += 2 * MOVEX;
                else if (tempMenuPosition.X > swapMenu.Position.X)
                    tempMenuPosition.X -= 2 * MOVEX;

                if (tempMenuPosition.Y < swapMenu.Position.Y)
                    tempMenuPosition.Y += 2 * MOVEY;
                else if (tempMenuPosition.Y > swapMenu.Position.Y)
                    tempMenuPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_EquipWeaponMoveInMode()
        {
            if (tempMenuPosition == equipWeaponMenu.Position)
                EnterEquipWeaponMode();
            else
            {
                if (tempMenuPosition.X < equipWeaponMenu.Position.X)
                    tempMenuPosition.X += 2 * MOVEX;
                else if (tempMenuPosition.X > equipWeaponMenu.Position.X)
                    tempMenuPosition.X -= 2 * MOVEX;

                if (tempMenuPosition.Y < equipWeaponMenu.Position.Y)
                    tempMenuPosition.Y += 2 * MOVEY;
                else if (tempMenuPosition.Y > equipWeaponMenu.Position.Y)
                    tempMenuPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_EquipWeaponMoveOutMode()
        {
            if (tempMenuPosition == equipWeaponMenu.Position)
                EnterBattleMenuMode();
            else
            {
                if (tempMenuPosition.X < equipWeaponMenu.Position.X)
                    tempMenuPosition.X += 2 * MOVEX;
                else if (tempMenuPosition.X > equipWeaponMenu.Position.X)
                    tempMenuPosition.X -= 2 * MOVEX;

                if (tempMenuPosition.Y < equipWeaponMenu.Position.Y)
                    tempMenuPosition.Y += 2 * MOVEY;
                else if (tempMenuPosition.Y > equipWeaponMenu.Position.Y)
                    tempMenuPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_EquipRingMoveInMode()
        {
            if (tempMenuPosition == equipRingMenu.Position)
                EnterEquipRingMode();
            else
            {
                if (tempMenuPosition.X < equipRingMenu.Position.X)
                    tempMenuPosition.X += 2 * MOVEX;
                else if (tempMenuPosition.X > equipRingMenu.Position.X)
                    tempMenuPosition.X -= 2 * MOVEX;

                if (tempMenuPosition.Y < equipRingMenu.Position.Y)
                    tempMenuPosition.Y += 2 * MOVEY;
                else if (tempMenuPosition.Y > equipRingMenu.Position.Y)
                    tempMenuPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_EquipRingMoveOutMode()
        {
            if (tempMenuPosition == equipRingMenu.Position)
                EnterEquipWeaponMode();
            else
            {
                if (tempMenuPosition.X < equipRingMenu.Position.X)
                    tempMenuPosition.X += 2 * MOVEX;
                else if (tempMenuPosition.X > equipRingMenu.Position.X)
                    tempMenuPosition.X -= 2 * MOVEX;

                if (tempMenuPosition.Y < equipRingMenu.Position.Y)
                    tempMenuPosition.Y += 2 * MOVEY;
                else if (tempMenuPosition.Y > equipRingMenu.Position.Y)
                    tempMenuPosition.Y -= 2 * MOVEY;
            }
        }

        private void UpdateVariables_PlayerBattleMode()
        {
            int i;
            int difference;

            // **** the spell boost has been implemented, still has to be tested
            // *** the spells muddle, dispel, desoul, slow have to be implemented
            // *** special attacks have to be implemented

            switch (battleModeState)
            {
                case BattleModeStates.InitialMessage:
                    battleModeState++;
                    expEarned = 0;
                    goldFound = 0;
                    secondAttack = false;
                    switch (battleMenu.CurrentState)
                    {
                        case BattleMenu.States.AttackSelected1:
                        case BattleMenu.States.AttackSelected2:
                            textMessage[0] = party.Members[party.MemberOnTurn].Name + "'s attack!";
                            textMessageLength = 1;
                            break;

                        case BattleMenu.States.MagicSelected1:
                        case BattleMenu.States.MagicSelected2:
                            switch (selectedMagicSpell.AreaEffectType)
                            {
                                case Spell.AreaEffectTypes.Default:
                                    textMessage[0] = party.Members[party.MemberOnTurn].Name + " casts";
                                    break;

                                case Spell.AreaEffectTypes.Divide:
                                    textMessage[0] = party.Members[party.MemberOnTurn].Name + " summons";
                                    break;
                            }
                            textMessage[1] = selectedMagicSpell.Name + " level " + selectedMagicLevel.ToString() + "!";
                            textMessageLength = 2;
                            break;

                        case BattleMenu.States.ItemSelected1:
                        case BattleMenu.States.ItemSelected2:
                            textMessage[0] = party.Members[party.MemberOnTurn].Name + " uses";
                            textMessage[1] = party.Members[party.MemberOnTurn].Items[selectedItemNumber].Name1 + party.Members[party.MemberOnTurn].Items[selectedItemNumber].Name2 + "!";
                            textMessageLength = 2;
                            break;
                    }
                    returnGameMode = GameModes.PlayerBattleMode;
                    waitForKeyPress = true;
                    moveOut = true;
                    EnterPlayerBattleDisplayTextMessageMode();
                    break;

                case BattleModeStates.SubtractMagicPoints:
                    battleModeState++;
                    if (battleMenu.CurrentState == BattleMenu.States.MagicSelected1 || battleMenu.CurrentState == BattleMenu.States.MagicSelected2)
                        party.Members[party.MemberOnTurn].MagicPoints -= selectedMagicSpell.MagicPoints[selectedMagicLevel - 1];
                    break;

                case BattleModeStates.CalculateDamage:
                    battleModeState++;
                    switch (battleMenu.CurrentState)
                    {
                        case BattleMenu.States.AttackSelected1:
                        case BattleMenu.States.AttackSelected2:
                            damageModifier = 0.1f - random.Next(20) / 100f;
                            if (damageModifier < 0)
                                damageModifier = 0;
                            int dodgedChance;
                            switch (targetsAttackedOrHealed[currentTargetAttackedOrHealed].BelongsToSide)
                            {
                                case CharacterPointer.Sides.Player:
                                    damage = (int)((float)party.Members[party.MemberOnTurn].AttackPoints * (1 + damageModifier)) - party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].DefensePoints;
                                    if (damage < 1) damage = 1;
                                    dodgedChance = random.Next(100);
                                    if (dodgedChance % 3 == 0)
                                        damage = 0;
                                    party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints -= damage;
                                    if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints < 0)
                                        party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints = 0;
                                    break;

                                case CharacterPointer.Sides.CPU_Opponents:
                                    damage = (int)((float)party.Members[party.MemberOnTurn].AttackPoints * (1 + damageModifier)) - enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].DefensePoints;
                                    if (damage < 1) damage = 1;
                                    dodgedChance = random.Next(100);
                                    if (dodgedChance == 49 || dodgedChance == 99)
                                        damage = 0;
                                    enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints -= damage;
                                    if (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints < 0)
                                        enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints = 0;
                                    break;
                            }
                            break;

                        case BattleMenu.States.MagicSelected1:
                        case BattleMenu.States.MagicSelected2:
                        case BattleMenu.States.ItemSelected1:
                        case BattleMenu.States.ItemSelected2:
                            switch (selectedMagicSpell.Type)
                            {
                                case Spell.Types.Attack:
                                    switch (targetsAttackedOrHealed[currentTargetAttackedOrHealed].BelongsToSide)
                                    {
                                        case CharacterPointer.Sides.Player:
                                            damageModifier = 0.05f - random.Next(10) / 100f;
                                            damage = (int)(selectedMagicSpell.EffectPoints[selectedMagicLevel - 1] * (1 + damageModifier));
                                            if (selectedMagicSpell.Element == Spell.Elements.Fire)
                                                if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].SusceptibleFire)
                                                    damage += damage / 2;
                                                else if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].ResistantFire)
                                                    damage -= damage / 2;
                                            if (selectedMagicSpell.Element == Spell.Elements.Ice)
                                                if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].SusceptibleIce)
                                                    damage += damage / 2;
                                                else if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].ResistantIce)
                                                    damage -= damage / 2;
                                            if (damage < 1) damage = 1;
                                            party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints -= damage;
                                            if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints < 0)
                                                party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints = 0;
                                            break;

                                        case CharacterPointer.Sides.CPU_Opponents:
                                            damageModifier = 0.05f - random.Next(10) / 100f;
                                            damage = (int)(selectedMagicSpell.EffectPoints[selectedMagicLevel - 1] * (1 + damageModifier));
                                            if (selectedMagicSpell.Element == Spell.Elements.Fire)
                                                if (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].SusceptibleFire)
                                                    damage += damage / 2;
                                                else if (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].ResistantFire)
                                                    damage -= damage / 2;
                                            if (selectedMagicSpell.Element == Spell.Elements.Ice)
                                                if (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].SusceptibleIce)
                                                    damage += damage / 2;
                                                else if (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].ResistantIce)
                                                    damage -= damage / 2;
                                            if (damage < 1) damage = 1;
                                            enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints -= damage;
                                            if (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints < 0)
                                                enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints = 0;
                                            break;
                                    }
                                    break;

                                case Spell.Types.Heal:
                                    switch (selectedMagicSpell.Name)
                                    {
                                        case "BOOST":
                                            if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].AgilityBoostedBy == 0 && party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].DefenseBoostedBy == 0)
                                            {
                                                int boostModifier = random.Next(30) - 15;
                                                if (boostModifier < 0)
                                                    boostModifier = 0;
                                                party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].AgilityBoostedBy = 15 + boostModifier;
                                                party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Agility += party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].AgilityBoostedBy;
                                                boostModifier = random.Next(30) - 15;
                                                if (boostModifier < 0)
                                                    boostModifier = 0;
                                                party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].DefenseBoostedBy = 15 + boostModifier;
                                                party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].DefensePoints += party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].DefenseBoostedBy;
                                                party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Boosted = 3;
                                                spellHasNoEffect = false;
                                            }
                                            else
                                                spellHasNoEffect = true;
                                            break;

                                        case "DETOX":
                                            party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Poisoned = false;
                                            break;

                                        default:
                                            damage = selectedMagicSpell.EffectPoints[selectedMagicLevel - 1];
                                            if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints + selectedMagicSpell.EffectPoints[selectedMagicLevel - 1] > party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].MaxHitPoints)
                                                damage = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].MaxHitPoints - party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints;
                                            party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints += damage;
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }
                    // *** here comes the animation
                    break;

                case BattleModeStates.DisplayDamageAndCalculateExperiencePointsAndGold:
                    battleModeState++;
                    if ((battleMenu.CurrentState == BattleMenu.States.MagicSelected1 || battleMenu.CurrentState == BattleMenu.States.MagicSelected2)
                        && party.Members[party.MemberOnTurn].Silenced > 0)
                    {
                        textMessage[0] = party.Members[party.MemberOnTurn].Name + " is silenced.";
                        textMessageLength = 1;
                        battleModeState = BattleModeStates.NextCharacter;
                    }
                    else
                    {
                        if (((battleMenu.CurrentState == BattleMenu.States.MagicSelected1 || battleMenu.CurrentState == BattleMenu.States.MagicSelected2)
                              && selectedMagicSpell.Type == Spell.Types.Heal)
                            || ((battleMenu.CurrentState == BattleMenu.States.ItemSelected1 || battleMenu.CurrentState == BattleMenu.States.ItemSelected2)
                                 && selectedMagicSpell.Type == Spell.Types.Heal))
                        {
                            switch (selectedMagicSpell.Name)
                            {
                                case "BOOST":
                                    if (spellHasNoEffect)
                                    {
                                        textMessage[0] = "The spell has no effect on";
                                        textMessage[1] = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + ".";
                                        textMessageLength = 2;
                                    }
                                    else
                                    {
                                        textMessage[0] = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + "'s agility";
                                        textMessage[1] = "increases by " + party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].AgilityBoostedBy.ToString() + ".";
                                        textMessage[2] = "Defense increases";
                                        textMessage[3] = "by " + party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].DefenseBoostedBy.ToString() + ".";
                                        textMessageLength = 4;
                                        expEarned += 5;
                                        if (expEarned > 50)
                                            expEarned = 47 + random.Next(6) - 3;
                                    }
                                    break;

                                case "DETOX":
                                    textMessage[0] = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " is no longer";
                                    textMessage[1] = "poisoned.";
                                    textMessageLength = 2;
                                    expEarned += 5;
                                    if (expEarned > 50)
                                        expEarned = 47 + random.Next(6) - 3;
                                    break;

                                default:
                                    textMessage[0] = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " recovers";
                                    textMessage[1] = damage.ToString() + " hit points.";
                                    textMessageLength = 2;
                                    expEarned += damage;
                                    if (expEarned < 9)
                                        expEarned = 9;
                                    else if (expEarned > 50)
                                        expEarned = 47 + random.Next(6) - 3;
                                    break;
                            }
                        }
                        else
                        {
                            if (damage == 0)
                            {
                                expEarned += 1;
                                if (party.Members[party.MemberOnTurn].Confused > 0)
                                    textMessage[0] = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " quickly";
                                else
                                    textMessage[0] = enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " quickly";
                                textMessage[1] = "dodged the attack!";
                                textMessageLength = 2;
                            }
                            else
                            {
                                i = 0;
                                if (damageModifier > 0.08f)
                                {
                                    textMessage[0] = "Critical hit!!";
                                    i = 1;
                                }
                                switch (targetsAttackedOrHealed[currentTargetAttackedOrHealed].BelongsToSide)
                                {
                                    case CharacterPointer.Sides.Player:
                                        textMessage[i] = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " gets";
                                        break;

                                    case CharacterPointer.Sides.CPU_Opponents:
                                        textMessage[i] = enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " gets";
                                        break;
                                }
                                textMessage[i + 1] = "damaged by " + damage.ToString() + ".";
                                textMessageLength = i + 2;
                                switch (targetsAttackedOrHealed[currentTargetAttackedOrHealed].BelongsToSide)
                                {
                                    case CharacterPointer.Sides.Player:
                                        if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints == 0)
                                        {
                                            textMessage[i + 2] = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " is";
                                            textMessage[i + 3] = "exhausted...";
                                            textMessageLength = i + 4;
                                            party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].NumDefeats++;
                                        }
                                        expEarned += 1;
                                        break;

                                    case CharacterPointer.Sides.CPU_Opponents:
                                        if (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints == 0)
                                        {
                                            textMessage[i + 2] = enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " is";
                                            textMessage[i + 3] = "defeated!";
                                            textMessageLength = i + 4;
                                            expEarned += (int)(((float)25 * (1 + 0.2f * (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Level - party.Members[party.MemberOnTurn].Level))) * (1 + (random.Next(20) - 10) / 100f));
                                            goldFound += enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Gold;
                                            party.Members[party.MemberOnTurn].NumKills++;
                                            if (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].ItemToLose() != -1
                                                && party.Members[party.MemberOnTurn].FreeItemSlot() != -1)
                                            {
                                                int tempItemNumber = enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].ItemToLose();
                                                party.Members[party.MemberOnTurn].Items[party.Members[party.MemberOnTurn].FreeItemSlot()] = enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Items[tempItemNumber];
                                                textMessage[i + 4] = enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " drops";
                                                textMessage[i + 5] = "a " + enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Items[tempItemNumber].Name1 + enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Items[tempItemNumber].Name2 + ".";
                                                textMessage[i + 6] = party.Members[party.MemberOnTurn].Name + " receives";
                                                textMessage[i + 7] = "the " + enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Items[tempItemNumber].Name1 + enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Items[tempItemNumber].Name2 + ".";
                                                textMessageLength = i + 8;
                                            }
                                        }
                                        else
                                        {
                                            if (damage < 25)
                                                expEarned += (int)(((float)damage * (1 + 0.2f * (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Level - party.Members[party.MemberOnTurn].Level))) * (1 + (random.Next(20) - 10) / 100f));
                                            else
                                                expEarned += (int)(((float)25 * (1 + 0.2f * (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Level - party.Members[party.MemberOnTurn].Level))) * (1 + (random.Next(20) - 10) / 100f));
                                        }
                                        if (expEarned > 50)
                                            expEarned = 47 + random.Next(6) - 3;
                                        else if (expEarned < 1)
                                            expEarned = 1;
                                        break;
                                }
                            }
                        }

                        if (currentTargetAttackedOrHealed < numTargetsAttackedOrHealed - 1)
                        {
                            currentTargetAttackedOrHealed++;
                            battleModeState = BattleModeStates.CalculateDamage;
                        }
                    }
                    returnGameMode = GameModes.PlayerBattleMode;
                    waitForKeyPress = true;
                    moveOut = true;
                    EnterPlayerBattleDisplayTextMessageMode();
                    break;

                case BattleModeStates.UpdateItemStatusAndCheckSecondAttack:
                    battleModeState++;
                    if (battleMenu.CurrentState == BattleMenu.States.ItemSelected1 || battleMenu.CurrentState == BattleMenu.States.ItemSelected2)
                    {
                        if (party.Members[party.MemberOnTurn].Items[selectedItemNumber].CanGetBroken)
                        {
                            party.Members[party.MemberOnTurn].Items[selectedItemNumber].UsesBeforeBroken--;
                            switch (party.Members[party.MemberOnTurn].Items[selectedItemNumber].UsesBeforeBroken)
                            {
                                case 1:
                                    // *** replace temporary text message with correct text message from the original game
                                    textMessage[0] = "But smoke rose from";
                                    textMessage[1] = "the " + party.Members[party.MemberOnTurn].Items[selectedItemNumber].Name1 + party.Members[party.MemberOnTurn].Items[selectedItemNumber].Name2 + ".";
                                    textMessageLength = 2;
                                    returnGameMode = GameModes.PlayerBattleMode;
                                    waitForKeyPress = true;
                                    moveOut = true;
                                    EnterPlayerBattleDisplayTextMessageMode();
                                    break;

                                case 0:
                                    // *** replace temporary text message with correct text message from the original game
                                    textMessage[0] = party.Members[party.MemberOnTurn].Items[selectedItemNumber].Name1 + party.Members[party.MemberOnTurn].Items[selectedItemNumber].Name2 + " is";
                                    textMessage[1] = "broken...";
                                    textMessageLength = 2;
                                    returnGameMode = GameModes.PlayerBattleMode;
                                    waitForKeyPress = true;
                                    moveOut = true;
                                    EnterPlayerBattleDisplayTextMessageMode();
                                    break;
                            }
                        }
                    }
                    else if (battleMenu.CurrentState == BattleMenu.States.AttackSelected1 || battleMenu.CurrentState == BattleMenu.States.AttackSelected2)
                    {
                        if (targetsAttackedOrHealed[currentTargetAttackedOrHealed].BelongsToSide == CharacterPointer.Sides.CPU_Opponents
                            && enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints > 0
                            && !secondAttack)
                        {
                            int secondAttackChance = random.Next(100);
                            if (secondAttackChance == 38 || secondAttackChance == 88)
                            {
                                secondAttack = true;
                                battleModeState = BattleModeStates.CalculateDamage;
                                textMessage[0] = party.Members[party.MemberOnTurn].Name + "'s second";
                                textMessage[1] = "attack!";
                                textMessageLength = 2;
                                returnGameMode = GameModes.PlayerBattleMode;
                                waitForKeyPress = true;
                                moveOut = true;
                                EnterPlayerBattleDisplayTextMessageMode();
                            }
                        }
                    }
                    break;

                case BattleModeStates.CounterAttackInitialMessage:
                    battleModeState = BattleModeStates.DisplayExperiencePointsAndGold;
                    if ((battleMenu.CurrentState == BattleMenu.States.AttackSelected1 || battleMenu.CurrentState == BattleMenu.States.AttackSelected2) && targetsAttackedOrHealed[currentTargetAttackedOrHealed].BelongsToSide == CharacterPointer.Sides.CPU_Opponents && enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints > 0)
                    {
                        int counterAttackChance = random.Next(100);                     
                        if (counterAttackChance == 27 || counterAttackChance == 77
                            || (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Boss && counterAttackChance % 2 == 0))
                        {
                            map.EmptyMapMarked();
                            for (i = enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].MinAttackRange; i <= enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].MaxAttackRange; i++)
                                map.MarkFieldsWithDistance(enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Position, i);
                            skip = 0;
                            bool charFound = false;
                            while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                                if (map.GetNextCharacterNumberLocatedInMarkedFields(party, skip) == party.MemberOnTurn)
                                    charFound = true;
                                else
                                    skip++;

                            map.EmptyMapMarked();
                            for (i = party.Members[party.MemberOnTurn].MinAttackRange; i <= party.Members[party.MemberOnTurn].MaxAttackRange; i++)
                                map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);

                            if (charFound)
                            {
                                battleModeState = BattleModeStates.CounterAttackCalculateDamage;
                                textMessage[0] = enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + "'s counter";
                                textMessage[1] = "attack!";
                                textMessageLength = 2;
                                returnGameMode = GameModes.PlayerBattleMode;
                                waitForKeyPress = true;
                                moveOut = true;
                                EnterPlayerBattleDisplayTextMessageMode();
                            }
                        }
                    }
                    break;

                case BattleModeStates.CounterAttackCalculateDamage:
                    battleModeState++;
                    damageModifier = 0.1f - random.Next(20) / 100f;
                    if (damageModifier < 0)
                        damageModifier = 0;
                    damage = (int)((float)enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].AttackPoints * (1 + damageModifier)) - party.Members[party.MemberOnTurn].DefensePoints;
                    if (damage < 1) damage = 1;
                    int dodgedChance1 = random.Next(100);
                    if (dodgedChance1 == 49 || dodgedChance1 == 99)
                        damage = 0;
                    party.Members[party.MemberOnTurn].HitPoints -= damage;
                    if (party.Members[party.MemberOnTurn].HitPoints < 0)
                        party.Members[party.MemberOnTurn].HitPoints = 0;

                    // *** here comes the animation
                    break;

                case BattleModeStates.CounterAttackDisplayDamage:
                    battleModeState++;
                    if (damage == 0)
                    {
                        textMessage[0] = party.Members[party.MemberOnTurn].Name + " quickly";
                        textMessage[1] = "dodged the attack!";
                        textMessageLength = 2;
                    }
                    else
                    {
                        i = 0;
                        if (damageModifier > 0.08f)
                        {
                            textMessage[0] = "Critical hit!!";
                            i = 1;
                        }
                        textMessage[i] = party.Members[party.MemberOnTurn].Name + " gets";
                        textMessage[i + 1] = "damaged by " + damage.ToString() + ".";
                        textMessageLength = i + 2;
                        if (party.Members[party.MemberOnTurn].HitPoints == 0)
                        {
                            textMessage[i + 2] = party.Members[party.MemberOnTurn].Name + " is";
                            textMessage[i + 3] = "exhausted...";
                            textMessageLength = i + 4;
                            party.Members[party.MemberOnTurn].NumDefeats++;
                        }
                    }
                    returnGameMode = GameModes.PlayerBattleMode;
                    waitForKeyPress = true;
                    moveOut = true;
                    EnterPlayerBattleDisplayTextMessageMode();
                    break;

                case BattleModeStates.CounterAttackDisplayExperiencePointsAndGold:
                    battleModeState++;
                    break;

                case BattleModeStates.DisplayExperiencePointsAndGold:
                    battleModeState++;
                    if (party.Members[party.MemberOnTurn].HitPoints > 0)
                    {
                        i = 2;
                        textMessage[0] = party.Members[party.MemberOnTurn].Name + " earns " + expEarned.ToString();
                        textMessage[1] = "EXP. points.";
                        textMessageLength = 2;
                        party.Members[party.MemberOnTurn].ExperiencePoints += expEarned;
                        if (party.Members[party.MemberOnTurn].ExperiencePoints >= 100)
                        {
                            party.Members[party.MemberOnTurn].ExperiencePoints -= 100;
                            LevelUpMessage levelUpMessage = party.Members[party.MemberOnTurn].NextLevel();
                            textMessage[2] = party.Members[party.MemberOnTurn].Name + " becomes";
                            textMessage[3] = "level " + party.Members[party.MemberOnTurn].LevelToDisplay + "!";
                            i = 4;
                            if (party.Members[party.MemberOnTurn].MaxHitPoints != party.Members[party.MemberOnTurn].OldMaxHitPoints)
                            {
                                difference = party.Members[party.MemberOnTurn].MaxHitPoints - party.Members[party.MemberOnTurn].OldMaxHitPoints;
                                textMessage[i] = "HP increase by " + difference.ToString() + "!";
                                i++;
                            }
                            if (party.Members[party.MemberOnTurn].MaxMagicPoints != party.Members[party.MemberOnTurn].OldMaxMagicPoints)
                            {
                                difference = party.Members[party.MemberOnTurn].MaxMagicPoints - party.Members[party.MemberOnTurn].OldMaxMagicPoints;
                                textMessage[i] = "MP increase by " + difference.ToString() + "!";
                                i++;
                            }
                            if (party.Members[party.MemberOnTurn].AttackPoints != party.Members[party.MemberOnTurn].OldAttackPoints)
                            {
                                difference = party.Members[party.MemberOnTurn].AttackPoints - party.Members[party.MemberOnTurn].OldAttackPoints;
                                textMessage[i] = "Attack increases by " + difference.ToString() + "!";
                                i++;
                            }
                            if (party.Members[party.MemberOnTurn].DefensePoints != party.Members[party.MemberOnTurn].OldDefensePoints)
                            {
                                difference = party.Members[party.MemberOnTurn].DefensePoints - party.Members[party.MemberOnTurn].OldDefensePoints;
                                textMessage[i] = "Defense increases by " + difference.ToString() + "!";
                                i++;
                            }
                            if (party.Members[party.MemberOnTurn].Agility != party.Members[party.MemberOnTurn].OldAgility)
                            {
                                difference = party.Members[party.MemberOnTurn].Agility - party.Members[party.MemberOnTurn].OldAgility;
                                textMessage[i] = "Agility increases by " + difference.ToString() + "!";
                                i++;
                            }
                            if (levelUpMessage.Message == LevelUpMessage.Messages.NewMagicSpell)
                            {
                                textMessage[i] = party.Members[party.MemberOnTurn].Name + " learns the new";
                                i++;
                                textMessage[i] = "magic spell " + party.Members[party.MemberOnTurn].MagicSpells[levelUpMessage.Number].Name + "!";
                                i++;
                            }
                            else if (levelUpMessage.Message == LevelUpMessage.Messages.MagicSpellLevelIncreased)
                            {
                                textMessage[i] = party.Members[party.MemberOnTurn].MagicSpells[levelUpMessage.Number].Name + " increases to";
                                i++;
                                textMessage[i] = "level " + party.Members[party.MemberOnTurn].MagicSpells[levelUpMessage.Number].Level + "!";
                                i++;
                            }
                            textMessageLength = i;
                        }
                        if ((battleMenu.CurrentState == BattleMenu.States.AttackSelected1 || battleMenu.CurrentState == BattleMenu.States.AttackSelected2
                             || ((battleMenu.CurrentState == BattleMenu.States.MagicSelected1 || battleMenu.CurrentState == BattleMenu.States.MagicSelected2) && selectedMagicSpell.Type == Spell.Types.Attack)
                             || ((battleMenu.CurrentState == BattleMenu.States.ItemSelected1 || battleMenu.CurrentState == BattleMenu.States.ItemSelected2) && selectedMagicSpell.Type == Spell.Types.Attack))
                            && enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints == 0)
                        {
                            textMessage[i] = "Finds " + goldFound.ToString() + " gold coins.";
                            gold += goldFound;
                            textMessageLength = i + 1;
                        }
                        returnGameMode = GameModes.PlayerBattleMode;
                        waitForKeyPress = true;
                        moveOut = true;
                        EnterPlayerBattleDisplayTextMessageMode();
                    }
                    break;

                case BattleModeStates.CheckIfAnyCharacterIsDefeated:
                    battleModeState++;
                    if ((battleMenu.CurrentState == BattleMenu.States.AttackSelected1 || battleMenu.CurrentState == BattleMenu.States.AttackSelected2
                         || ((battleMenu.CurrentState == BattleMenu.States.MagicSelected1 || battleMenu.CurrentState == BattleMenu.States.MagicSelected2) && selectedMagicSpell.Type == Spell.Types.Attack)
                         || ((battleMenu.CurrentState == BattleMenu.States.ItemSelected1 || battleMenu.CurrentState == BattleMenu.States.ItemSelected2) && selectedMagicSpell.Type == Spell.Types.Attack)))
                    {
                        bool opponentDefeated = false;
                        for (i = 0; i < numTargetsAttackedOrHealed; i++)
                            if (enemies.Members[targetsAttackedOrHealed[i].WhichOne].HitPoints == 0)
                            {
                                opponentDefeated = true;
                                break;
                            }
                        if (opponentDefeated || party.Members[party.MemberOnTurn].HitPoints == 0)
                        {
                            if (enemies.MustSurviveIsDefeated())
                                for (i = 0; i < enemies.NumPartyMembers; i++)
                                    enemies.Members[i].HitPoints = 0;
                            returnGameMode = GameModes.PlayerBattleMode;
                            EnterDisplayDefeatedCharactersMode();
                        }
                    }
                    if ((battleMenu.CurrentState == BattleMenu.States.ItemSelected1 || battleMenu.CurrentState == BattleMenu.States.ItemSelected2)
                        && ((party.Members[party.MemberOnTurn].Items[selectedItemNumber].CanGetBroken && party.Members[party.MemberOnTurn].Items[selectedItemNumber].UsesBeforeBroken == 0) || party.Members[party.MemberOnTurn].Items[selectedItemNumber].CanBeUsedOnlyOnce))
                    {
                        party.Members[party.MemberOnTurn].Items[selectedItemNumber] = null;
                        party.Members[party.MemberOnTurn].RearrangeItems(selectedItemNumber);
                    }
                    break;

                case BattleModeStates.NextCharacter:
                    party.Members[party.MemberOnTurn].LookDown();
                    EnterEndTurnOfCharacterAndNextCharacterMode();
                    break;
            }
        }

        private void UpdateVariables_DisplayDefeatedCharactersMode()
        {
            for (int i = 0; i < party.NumPartyMembers; i++)
                if (party.Members[i].HitPoints == 0)
                    party.Members[i].RotateClockwise();

            for (int i = 0; i < enemies.NumPartyMembers; i++)
                if (enemies.Members[i].HitPoints == 0)
                    enemies.Members[i].RotateClockwise();

            displayDefeatedCharactersModeState++;

            if (displayDefeatedCharactersModeState == DEFEAT_DISPLAY_WAIT_DURATION + DEFEAT_DISPLAY_EXPLOSION_DURATION)
            {
                for (int i = 0; i < party.NumPartyMembers; i++)
                    if (party.Members[i].HitPoints == 0)
                        party.Members[i].Alive = false;

                for (int i = 0; i < enemies.NumPartyMembers; i++)
                    if (enemies.Members[i].HitPoints == 0)
                        enemies.Members[i].Alive = false;

                ReturnToReturnGameMode();
            }
        }

        private void UpdateVariables_NextCharacterAfterSleepMode()
        {
            selectionBar.Position = map.CalcPosition(party.Members[party.MemberOnTurn].Position);

            party.Members[party.MemberOnTurn].LookDown();
            EnterEndTurnOfCharacterAndNextCharacterMode();
        }

        private void UpdateVariables_PlayerBattleDisplayTextMessageMode()
        {
            UpdateVariables_DisplayTextMessageMode();
        }


        private void UpdateVariables_BattleDisplayTextMessageMoveOutMode()
        {
            UpdateVariables_DisplayTextMessageMoveOutMode();
        }

        private void UpdateVariables_EnemyMoveTransitionMode()
        {
            if (oldMapPosition == map.Position && oldSelectionBarPosition == selectionBar.Position)
                EnterEnemyMoveMode();

            if (oldMapPosition != map.Position)
            {
                if (oldMapPosition.X < map.Position.X)
                {
                    oldMapPosition.X++;
                    map.Offset.X = Map.TILESIZEX;
                }
                else if (oldMapPosition.X > map.Position.X)
                {
                    oldMapPosition.X--;
                    map.Offset.X = -Map.TILESIZEX;
                }

                if (oldMapPosition.Y < map.Position.Y)
                {
                    oldMapPosition.Y++;
                    map.Offset.Y = Map.TILESIZEY;
                }
                else if (oldMapPosition.Y > map.Position.Y)
                {
                    oldMapPosition.Y--;
                    map.Offset.Y = -Map.TILESIZEY;
                }

                tempSelectionBarPosition.X = oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX;
                tempSelectionBarPosition.Y = oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY;

                GameMode = GameModes.TransitionInEnemyMoveTransitionMode;
            }

            if (oldSelectionBarPosition != selectionBar.Position)
            {
                tempSelectionBarPosition.X = oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX;
                tempSelectionBarPosition.Y = oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY;

                if (oldSelectionBarPosition.X < selectionBar.Position.X)
                    oldSelectionBarPosition.X++;
                else if (oldSelectionBarPosition.X > selectionBar.Position.X)
                    oldSelectionBarPosition.X--;

                if (oldSelectionBarPosition.Y < selectionBar.Position.Y)
                    oldSelectionBarPosition.Y++;
                else if (oldSelectionBarPosition.Y > selectionBar.Position.Y)
                    oldSelectionBarPosition.Y--;

                GameMode = GameModes.TransitionInEnemyMoveTransitionMode;
            }
        }

        private void UpdateVariables_TransitionInEnemyMoveTransitionMode()
        {
            bool mapOkay = false;

            if (map.Offset.X == 0 && map.Offset.Y == 0)
                mapOkay = true;
            else
            {
                if (map.Offset.X < 0)
                    map.Offset.X += 2 * MOVEX;
                else if (map.Offset.X > 0)
                    map.Offset.X -= 2 * MOVEX;

                if (map.Offset.Y < 0)
                    map.Offset.Y += 2 * MOVEY;
                else if (map.Offset.Y > 0)
                    map.Offset.Y -= 2 * MOVEY;
            }

            if (tempSelectionBarPosition.X != oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX
                || tempSelectionBarPosition.Y != oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
            {
                if (tempSelectionBarPosition.X < oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X += 2 * MOVEX;
                else if (tempSelectionBarPosition.X > oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X -= 2 * MOVEX;

                if (tempSelectionBarPosition.Y < oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y += 2 * MOVEY;
                else if (tempSelectionBarPosition.Y > oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y -= 2 * MOVEY;
            }
            else if (mapOkay == true)
                GameMode = GameModes.EnemyMoveTransitionMode;
        }

        private void UpdateVariables_EnemyMoveMode()
        {
            if (enemies.Members[enemies.MemberOnTurn].Position != backUpPosition)
            {
                EnterEnemyMovingMode();
                switch (map.bestPath[currentStepNumber])
                {
                    case Map.Directions.Up:
                        enemies.Members[enemies.MemberOnTurn].Position -= map.Size.X;
                        enemies.Members[enemies.MemberOnTurn].LookUp();
                        break;

                    case Map.Directions.Left:
                        enemies.Members[enemies.MemberOnTurn].Position -= 1;
                        enemies.Members[enemies.MemberOnTurn].LookLeft();
                        break;

                    case Map.Directions.Right:
                        enemies.Members[enemies.MemberOnTurn].Position += 1;
                        enemies.Members[enemies.MemberOnTurn].LookRight();
                        break;

                    case Map.Directions.Down:
                        enemies.Members[enemies.MemberOnTurn].Position += map.Size.X;
                        enemies.Members[enemies.MemberOnTurn].LookDown();
                        break;
                }
                currentStepNumber++;
            }
            // **** this should be checked again when developed further
            else if ((enemyBattleMove == BattleMoves.Attack || enemyBattleMove == BattleMoves.MagicAttack || enemyBattleMove == BattleMoves.MagicHeal || enemyBattleMove == BattleMoves.ItemMagicAttack || enemyBattleMove == BattleMoves.ItemMagicHeal) && !map.IsVisible(targetPosition, map.Position))
                EnterEnemyMovingMode();
            else
            {
                switch (enemyBattleMove)
                {
                    case BattleMoves.Attack:
                    case BattleMoves.MagicAttack:
                    case BattleMoves.ItemMagicAttack:
                    case BattleMoves.MagicHeal:
                    case BattleMoves.ItemMagicHeal:
                        EnterEnemyAttackMode();
                        break;

                    case BattleMoves.Stay:
                        selectionBar.Position = map.CalcPosition(enemies.Members[enemies.MemberOnTurn].Position);
                        enemies.Members[enemies.MemberOnTurn].LookDown();
                        EnterEndTurnOfCharacterAndNextCharacterMode();
                        break;
                }
            }
        }

        private void UpdateVariables_EnemyMovingMode()
        {
            if (map.Offset.X != 0 || map.Offset.Y != 0)
            {
                if (map.Offset.X < 0)
                    map.Offset.X += MOVEX;
                else if (map.Offset.X > 0)
                    map.Offset.X -= MOVEX;

                if (map.Offset.Y < 0)
                    map.Offset.Y += MOVEY;
                else if (map.Offset.Y > 0)
                    map.Offset.Y -= MOVEY;
            }

            if (delayPosition.X == map.CalcPosition(enemies.Members[enemies.MemberOnTurn].Position).X * Map.TILESIZEX
                && delayPosition.Y == map.CalcPosition(enemies.Members[enemies.MemberOnTurn].Position).Y * Map.TILESIZEY
                && map.Offset.X == 0
                && map.Offset.Y == 0)
                GameMode = GameModes.EnemyMoveMode;
            else
            {
                if (delayPosition.X < map.CalcPosition(enemies.Members[enemies.MemberOnTurn].Position).X * Map.TILESIZEX)
                    delayPosition.X += MOVEX;
                else if (delayPosition.X > map.CalcPosition(enemies.Members[enemies.MemberOnTurn].Position).X * Map.TILESIZEX)
                    delayPosition.X -= MOVEX;

                if (delayPosition.Y < map.CalcPosition(enemies.Members[enemies.MemberOnTurn].Position).Y * Map.TILESIZEY)
                    delayPosition.Y += MOVEY;
                else if (delayPosition.Y > map.CalcPosition(enemies.Members[enemies.MemberOnTurn].Position).Y * Map.TILESIZEY)
                    delayPosition.Y -= MOVEY;
            }
        }

        private void UpdateVariables_EnemyBattleMode()
        {
            int i;
            int difference;

            // **** the spell boost has been implemented, still has to be tested
            // *** poisoning, silencing, stunning still have to be implemented
            // *** the spells muddle, dispel, desoul, slow have to be implemented
            // *** special attacks have to be implemented

            switch (battleModeState)
            {
                case BattleModeStates.InitialMessage:
                    battleModeState++;
                    expEarned = 0;
                    secondAttack = false;
                    switch (enemyBattleMove)
                    {
                        case BattleMoves.Attack:
                            textMessage[0] = enemies.Members[enemies.MemberOnTurn].Name + "'s attack!";
                            textMessageLength = 1;
                            break;

                        case BattleMoves.MagicAttack:
                        case BattleMoves.MagicHeal:
                            switch (enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].AreaEffectType)
                            {
                                case Spell.AreaEffectTypes.Default:
                                    textMessage[0] = enemies.Members[enemies.MemberOnTurn].Name + " cast";
                                    break;

                                case Spell.AreaEffectTypes.Divide:
                                    textMessage[0] = enemies.Members[enemies.MemberOnTurn].Name + " summoned";
                                    break;
                            }
                            textMessage[1] = selectedMagicSpell.Name + " level " + selectedMagicLevel.ToString() + "!";
                            textMessageLength = 2;
                            break;

                        case BattleMoves.ItemMagicAttack:
                        case BattleMoves.ItemMagicHeal:
                            textMessage[0] = enemies.Members[enemies.MemberOnTurn].Name + " uses";
                            textMessage[1] = enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].Name1 + enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].Name2 + "!";
                            textMessageLength = 2;
                            break;
                    }
                    returnGameMode = GameModes.EnemyBattleMode;
                    waitForKeyPress = true;
                    moveOut = true;
                    EnterEnemyBattleDisplayTextMessageMode();
                    break;

                case BattleModeStates.SubtractMagicPoints:
                    battleModeState++;
                    if (enemyBattleMove == BattleMoves.MagicAttack || enemyBattleMove == BattleMoves.MagicHeal)
                        enemies.Members[enemies.MemberOnTurn].MagicPoints -= selectedMagicSpell.MagicPoints[selectedMagicLevel - 1];
                    break;

                case BattleModeStates.CalculateDamage:
                    battleModeState++;
                    switch (enemyBattleMove)
                    {
                        case BattleMoves.Attack:
                            damageModifier = 0.1f - random.Next(20) / 100f;
                            if (damageModifier < 0)
                                damageModifier = 0;
                            int dodgedChance = random.Next(100);
                            switch (targetsAttackedOrHealed[currentTargetAttackedOrHealed].BelongsToSide)
                            {
                                case CharacterPointer.Sides.CPU_Opponents:
                                    damage = (int)((float)enemies.Members[enemies.MemberOnTurn].AttackPoints * (1 + damageModifier)) - enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].DefensePoints;
                                    if (damage < 1) damage = 1;
                                    if (dodgedChance % 3 == 0)
                                        damage = 0;
                                    enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints -= damage;
                                    if (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints < 0)
                                        enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints = 0;
                                    break;

                                case CharacterPointer.Sides.Player:
                                    damage = (int)((float)enemies.Members[enemies.MemberOnTurn].AttackPoints * (1 + damageModifier)) - party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].DefensePoints;
                                    if (damage < 1) damage = 1;
                                    if (dodgedChance == 49 || dodgedChance == 99)
                                        damage = 0;
                                    party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints -= damage;
                                    if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints < 0)
                                        party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints = 0;
                                    break;
                            }
                            break;

                        case BattleMoves.MagicAttack:
                        case BattleMoves.ItemMagicAttack:
                            switch (targetsAttackedOrHealed[currentTargetAttackedOrHealed].BelongsToSide)
                            {
                                case CharacterPointer.Sides.CPU_Opponents:
                                    damageModifier = 0.05f - random.Next(10) / 100f;
                                    damage = (int)(selectedMagicSpell.EffectPoints[selectedMagicLevel - 1] * (1 + damageModifier));
                                    if (selectedMagicSpell.Element == Spell.Elements.Fire)
                                        if (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].SusceptibleFire)
                                            damage += damage / 2;
                                        else if (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].ResistantFire)
                                            damage -= damage / 2;
                                    if (selectedMagicSpell.Element == Spell.Elements.Ice)
                                        if (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].SusceptibleIce)
                                            damage += damage / 2;
                                        else if (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].ResistantIce)
                                            damage -= damage / 2;
                                    if (damage < 1) damage = 1;
                                    enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints -= damage;
                                    if (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints < 0)
                                        enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints = 0;
                                    break;

                                case CharacterPointer.Sides.Player:
                                    damageModifier = 0.05f - random.Next(10) / 100f;
                                    damage = (int)(selectedMagicSpell.EffectPoints[selectedMagicLevel - 1] * (1 + damageModifier));
                                    if (selectedMagicSpell.Element == Spell.Elements.Fire)
                                        if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].SusceptibleFire)
                                            damage += damage / 2;
                                        else if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].ResistantFire)
                                            damage -= damage / 2;
                                    if (selectedMagicSpell.Element == Spell.Elements.Ice)
                                        if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].SusceptibleIce)
                                            damage += damage / 2;
                                        else if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].ResistantIce)
                                            damage -= damage / 2;
                                    if (damage < 1) damage = 1;
                                    party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints -= damage;
                                    if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints < 0)
                                        party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints = 0;
                                    break;
                            }
                            break;

                        case BattleMoves.MagicHeal:
                        case BattleMoves.ItemMagicHeal:
                            switch (selectedMagicSpell.Name)
                            {
                                case "BOOST":
                                    if (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].AgilityBoostedBy == 0 && enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].DefenseBoostedBy == 0)
                                    {
                                        int boostModifier = random.Next(30) - 15;
                                        if (boostModifier < 0)
                                            boostModifier = 0;
                                        enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].AgilityBoostedBy = 15 + boostModifier;
                                        enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Agility += enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].AgilityBoostedBy;
                                        boostModifier = random.Next(30) - 15;
                                        if (boostModifier < 0)
                                            boostModifier = 0;
                                        enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].DefenseBoostedBy = 15 + boostModifier;
                                        enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].DefensePoints += enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].DefenseBoostedBy;
                                        enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Boosted = 3;
                                        spellHasNoEffect = false;
                                    }
                                    else
                                        spellHasNoEffect = true;
                                    break;

                                case "DETOX":
                                    enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Poisoned = false;
                                    break;

                                default:
                                    damage = selectedMagicSpell.EffectPoints[selectedMagicLevel - 1];
                                    if (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints + selectedMagicSpell.EffectPoints[selectedMagicLevel - 1] > enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].MaxHitPoints)
                                        damage = enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].MaxHitPoints - enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints;
                                    enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints += damage;
                                    break;
                            }
                            break;
                    }
                    // *** here comes the animation
                    break;

                case BattleModeStates.DisplayDamageAndCalculateExperiencePointsAndGold:
                    battleModeState++;
                    if ((enemyBattleMove == BattleMoves.MagicAttack || enemyBattleMove == BattleMoves.MagicHeal)
                        && enemies.Members[enemies.MemberOnTurn].Silenced > 0)
                    {
                        textMessage[0] = enemies.Members[enemies.MemberOnTurn].Name + " is silenced.";
                        textMessageLength = 1;
                        battleModeState = BattleModeStates.NextCharacter;
                    }
                    else
                    {
                        if (enemyBattleMove == BattleMoves.MagicHeal || enemyBattleMove == BattleMoves.ItemMagicHeal)
                        {
                            switch (selectedMagicSpell.Name)
                            {
                                case "BOOST":
                                    if (spellHasNoEffect)
                                    {
                                        textMessage[0] = "The spell has no effect on";
                                        textMessage[1] = enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + ".";
                                        textMessageLength = 2;
                                    }
                                    else
                                    {

                                        textMessage[0] = enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + "'s agility";
                                        textMessage[1] = "increases by " + enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].AgilityBoostedBy.ToString() + ".";
                                        textMessage[2] = "Defense increases";
                                        textMessage[3] = "by " + enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].DefenseBoostedBy.ToString() + ".";
                                        textMessageLength = 4;
                                    }
                                    break;

                                case "DETOX":
                                    textMessage[0] = enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " is no longer";
                                    textMessage[1] = "poisoned.";
                                    textMessageLength = 2;
                                    break;

                                default:
                                    textMessage[0] = enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " recovers";
                                    textMessage[1] = damage.ToString() + " hit points.";
                                    textMessageLength = 2;
                                    break;
                            }
                        }
                        else if (enemyBattleMove == BattleMoves.Attack || enemyBattleMove == BattleMoves.MagicAttack || enemyBattleMove == BattleMoves.ItemMagicAttack)
                        {
                            if (damage == 0)
                            {
                                switch (targetsAttackedOrHealed[currentTargetAttackedOrHealed].BelongsToSide)
                                {
                                    case CharacterPointer.Sides.CPU_Opponents:
                                        textMessage[0] = enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " quickly";
                                        break;

                                    case CharacterPointer.Sides.Player:
                                        textMessage[0] = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " quickly";
                                        break;
                                }
                                textMessage[1] = "dodged the attack!";
                                textMessageLength = 2;
                            }
                            else
                            {
                                i = 0;
                                if (damageModifier > 0.08f)
                                {
                                    textMessage[0] = "Critical hit!!";
                                    i = 1;
                                }
                                switch (targetsAttackedOrHealed[currentTargetAttackedOrHealed].BelongsToSide)
                                {
                                    case CharacterPointer.Sides.CPU_Opponents:
                                        textMessage[i] = enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " gets";
                                        break;

                                    case CharacterPointer.Sides.Player:
                                        textMessage[i] = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " gets";
                                        break;
                                }
                                textMessage[i + 1] = "damaged by " + damage.ToString() + ".";
                                textMessageLength = i + 2;
                                switch (targetsAttackedOrHealed[currentTargetAttackedOrHealed].BelongsToSide)
                                {
                                    case CharacterPointer.Sides.CPU_Opponents:
                                        if (enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints == 0)
                                        {
                                            textMessage[i + 2] = enemies.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " is";
                                            textMessage[i + 3] = "defeated!";
                                            textMessageLength = i + 4;
                                        }
                                        break;

                                    case CharacterPointer.Sides.Player:
                                        if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints == 0)
                                        {
                                            textMessage[i + 2] = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " is";
                                            textMessage[i + 3] = "exhausted...";
                                            textMessageLength = i + 4;
                                            party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].NumDefeats++;
                                        }
                                        break;
                                }
                            }
                        }

                        if (currentTargetAttackedOrHealed < numTargetsAttackedOrHealed - 1)
                        {
                            currentTargetAttackedOrHealed++;
                            battleModeState = BattleModeStates.CalculateDamage;
                        }
                    }
                    returnGameMode = GameModes.EnemyBattleMode;
                    waitForKeyPress = true;
                    moveOut = true;
                    EnterEnemyBattleDisplayTextMessageMode();
                    break;

                case BattleModeStates.UpdateItemStatusAndCheckSecondAttack:
                    battleModeState++;
                    if (enemyBattleMove == BattleMoves.Attack)
                    {
                        if (targetsAttackedOrHealed[currentTargetAttackedOrHealed].BelongsToSide == CharacterPointer.Sides.Player
                            && party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints > 0
                            && !secondAttack)
                        {
                            int secondAttackChance = random.Next(100);
                            if (secondAttackChance == 38 || secondAttackChance == 88)
                            {
                                secondAttack = true;
                                battleModeState = BattleModeStates.CalculateDamage;
                                textMessage[0] = enemies.Members[enemies.MemberOnTurn].Name + "'s second";
                                textMessage[1] = "attack!";
                                textMessageLength = 2;
                                returnGameMode = GameModes.EnemyBattleMode;
                                waitForKeyPress = true;
                                moveOut = true;
                                EnterEnemyBattleDisplayTextMessageMode();
                            }
                        }
                    }
                    break;

                case BattleModeStates.CounterAttackInitialMessage:
                    battleModeState = BattleModeStates.DisplayExperiencePointsAndGold;
                    if (enemyBattleMove == BattleMoves.Attack && targetsAttackedOrHealed[currentTargetAttackedOrHealed].BelongsToSide == CharacterPointer.Sides.Player && party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].HitPoints > 0)
                    {
                        int counterAttackChance = random.Next(100);
                        if (counterAttackChance == 27 || counterAttackChance == 77
                            || (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Boss && counterAttackChance % 2 == 0))
                        {
                            map.EmptyMapMarked();
                            for (i = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].MinAttackRange; i <= party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].MaxAttackRange; i++)
                                map.MarkFieldsWithDistance(party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Position, i);
                            skip = 0;
                            bool charFound = false;
                            while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip) != -1)
                                if (map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip) == enemies.MemberOnTurn)
                                    charFound = true;
                                else
                                    skip++;

                            map.EmptyMapMarked();
                            for (i = enemies.Members[enemies.MemberOnTurn].MinAttackRange; i <= enemies.Members[enemies.MemberOnTurn].MaxAttackRange; i++)
                                map.MarkFieldsWithDistance(enemies.Members[enemies.MemberOnTurn].Position, i);

                            if (charFound)
                            {
                                battleModeState = BattleModeStates.CounterAttackCalculateDamage;
                                textMessage[0] = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + "'s counter";
                                textMessage[1] = "attack!";
                                textMessageLength = 2;
                                returnGameMode = GameModes.EnemyBattleMode;
                                waitForKeyPress = true;
                                moveOut = true;
                                EnterEnemyBattleDisplayTextMessageMode();
                            }
                        }
                    }
                    break;

                case BattleModeStates.CounterAttackCalculateDamage:
                    battleModeState++;
                    damageModifier = 0.1f - random.Next(20) / 100f;
                    if (damageModifier < 0)
                        damageModifier = 0;
                    damage = (int)((float)party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].AttackPoints * (1 + damageModifier)) - enemies.Members[enemies.MemberOnTurn].DefensePoints;
                    if (damage < 1) damage = 1;
                    int dodgedChance1 = random.Next(100);
                    if (dodgedChance1 == 49 || dodgedChance1 == 99)
                        damage = 0;
                    enemies.Members[enemies.MemberOnTurn].HitPoints -= damage;
                    if (enemies.Members[enemies.MemberOnTurn].HitPoints < 0)
                        enemies.Members[enemies.MemberOnTurn].HitPoints = 0;

                    // *** here comes the animation
                    break;

                case BattleModeStates.CounterAttackDisplayDamage:
                    battleModeState++;
                    if (damage == 0)
                    {
                        expEarned += 1;
                        textMessage[0] = enemies.Members[enemies.MemberOnTurn].Name + " quickly";
                        textMessage[1] = "dodged the attack!";
                        textMessageLength = 2;
                    }
                    else
                    {
                        i = 0;
                        if (damageModifier > 0.08f)
                        {
                            textMessage[0] = "Critical hit!!";
                            i = 1;
                        }
                        textMessage[i] = enemies.Members[enemies.MemberOnTurn].Name + " gets";
                        textMessage[i + 1] = "damaged by " + damage.ToString() + ".";
                        textMessageLength = i + 2;
                        if (enemies.Members[enemies.MemberOnTurn].HitPoints == 0)
                        {
                            textMessage[i + 2] = enemies.Members[enemies.MemberOnTurn].Name + " is";
                            textMessage[i + 3] = "defeated!";
                            textMessageLength = i + 4;
                            expEarned += (int)(((float)25 * (1 + 0.2f * (enemies.Members[enemies.MemberOnTurn].Level - party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Level))) * (1 + (random.Next(20) - 10) / 100f));
                            goldFound += enemies.Members[enemies.MemberOnTurn].Gold;
                            party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].NumKills++;
                            if (enemies.Members[enemies.MemberOnTurn].ItemToLose() != -1
                                && party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].FreeItemSlot() != -1)
                            {
                                int tempItemNumber = enemies.Members[enemies.MemberOnTurn].ItemToLose();
                                party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Items[party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].FreeItemSlot()] = enemies.Members[enemies.MemberOnTurn].Items[tempItemNumber];
                                textMessage[i + 4] = enemies.Members[enemies.MemberOnTurn].Name + " dropped";
                                textMessage[i + 5] = "a " + enemies.Members[enemies.MemberOnTurn].Items[tempItemNumber].Name1 + enemies.Members[enemies.MemberOnTurn].Items[tempItemNumber].Name2 + ".";
                                textMessage[i + 6] = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " receives";
                                textMessage[i + 7] = "the " + enemies.Members[enemies.MemberOnTurn].Items[tempItemNumber].Name1 + enemies.Members[enemies.MemberOnTurn].Items[tempItemNumber].Name2 + ".";
                                textMessageLength = i + 8;
                            }
                        }
                        else
                        {
                            if (damage < 25)
                                expEarned += (int)(((float)damage * (1 + 0.2f * (enemies.Members[enemies.MemberOnTurn].Level - party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Level))) * (1 + (random.Next(20) - 10) / 100f));
                            else
                                expEarned += (int)(((float)25 * (1 + 0.2f * (enemies.Members[enemies.MemberOnTurn].Level - party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Level))) * (1 + (random.Next(20) - 10) / 100f));
                        }
                        if (expEarned > 50)
                            expEarned = 47 + random.Next(6) - 3;
                        else if (expEarned < 1)
                            expEarned = 1;
                        if (enemies.MustSurviveIsDefeated())
                            for (i = 0; i < enemies.NumPartyMembers; i++)
                                enemies.Members[i].HitPoints = 0;
                    }
                    returnGameMode = GameModes.EnemyBattleMode;
                    waitForKeyPress = true;
                    moveOut = true;
                    EnterEnemyBattleDisplayTextMessageMode();
                    break;

                case BattleModeStates.CounterAttackDisplayExperiencePointsAndGold:
                    battleModeState++;
                    i = 2;
                    textMessage[0] = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " earns " + expEarned.ToString();
                    textMessage[1] = "EXP. points.";
                    textMessageLength = 2;
                    party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].ExperiencePoints += expEarned;
                    if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].ExperiencePoints >= 100)
                    {
                        party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].ExperiencePoints -= 100;
                        LevelUpMessage levelUpMessage = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].NextLevel();
                        textMessage[2] = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " becomes";
                        textMessage[3] = "level " + party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].LevelToDisplay + "!";
                        i = 4;
                        if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].MaxHitPoints != party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].OldMaxHitPoints)
                        {
                            difference = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].MaxHitPoints - party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].OldMaxHitPoints;
                            textMessage[i] = "HP increase by " + difference.ToString() + "!";
                            i++;
                        }
                        if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].MaxMagicPoints != party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].OldMaxMagicPoints)
                        {
                            difference = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].MaxMagicPoints - party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].OldMaxMagicPoints;
                            textMessage[i] = "MP increase by " + difference.ToString() + "!";
                            i++;
                        }
                        if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].AttackPoints != party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].OldAttackPoints)
                        {
                            difference = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].AttackPoints - party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].OldAttackPoints;
                            textMessage[i] = "Attack increases by " + difference.ToString() + "!";
                            i++;
                        }
                        if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].DefensePoints != party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].OldDefensePoints)
                        {
                            difference = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].DefensePoints - party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].OldDefensePoints;
                            textMessage[i] = "Defense increases by " + difference.ToString() + "!";
                            i++;
                        }
                        if (party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Agility != party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].OldAgility)
                        {
                            difference = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Agility - party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].OldAgility;
                            textMessage[i] = "Agility increases by " + difference.ToString() + "!";
                            i++;
                        }
                        if (levelUpMessage.Message == LevelUpMessage.Messages.NewMagicSpell)
                        {
                            textMessage[i] = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].Name + " learns the new";
                            i++;
                            textMessage[i] = "magic spell " + party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].MagicSpells[levelUpMessage.Number].Name + "!";
                            i++;
                        }
                        else if (levelUpMessage.Message == LevelUpMessage.Messages.MagicSpellLevelIncreased)
                        {
                            textMessage[i] = party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].MagicSpells[levelUpMessage.Number].Name + " increases to";
                            i++;
                            textMessage[i] = "level " + party.Members[targetsAttackedOrHealed[currentTargetAttackedOrHealed].WhichOne].MagicSpells[levelUpMessage.Number].Level + "!";
                            i++;
                        }
                        textMessageLength = i;
                    }
                    if (enemies.Members[enemies.MemberOnTurn].HitPoints == 0)
                    {
                        textMessage[i] = "Finds " + goldFound.ToString() + " gold coins.";
                        gold += goldFound;
                        textMessageLength = i + 1;
                    }
                    returnGameMode = GameModes.EnemyBattleMode;
                    waitForKeyPress = true;
                    moveOut = true;
                    EnterEnemyBattleDisplayTextMessageMode();
                    break;

                case BattleModeStates.DisplayExperiencePointsAndGold:
                    battleModeState++;
                    break;

                case BattleModeStates.CheckIfAnyCharacterIsDefeated:
                    battleModeState++;
                    if (enemyBattleMove == BattleMoves.Attack || enemyBattleMove == BattleMoves.MagicAttack || enemyBattleMove == BattleMoves.ItemMagicAttack)
                    {
                        bool opponentDefeated = false;
                        for (i = 0; i < numTargetsAttackedOrHealed; i++)
                            if (party.Members[targetsAttackedOrHealed[i].WhichOne].HitPoints == 0)
                            {
                                opponentDefeated = true;
                                break;
                            }
                        if (opponentDefeated)
                        {
                            returnGameMode = GameModes.EnemyBattleMode;
                            EnterDisplayDefeatedCharactersMode();
                        }
                    }
                    if ((enemyBattleMove == BattleMoves.ItemMagicAttack || enemyBattleMove == BattleMoves.ItemMagicHeal)
                        && enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].CanBeUsedOnlyOnce)
                    {
                        enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber] = null;
                        enemies.Members[enemies.MemberOnTurn].RearrangeItems(selectedItemNumber);
                    }
                    break;

                case BattleModeStates.NextCharacter:
                    enemies.Members[enemies.MemberOnTurn].LookDown();
                    EnterEndTurnOfCharacterAndNextCharacterMode();
                    break;
            }
        }

        private void UpdateVariables_EnemyBattleDisplayTextMessageMode()
        {
            UpdateVariables_PlayerBattleDisplayTextMessageMode();
        }

        private void UpdateVariables_EnemyAttackMode()
        {
            if (oldMapPosition != map.Position)
            {
                map.Offset.X = -Map.TILESIZEX * (oldMapPosition.X - map.Position.X);
                map.Offset.Y = -Map.TILESIZEY * (oldMapPosition.Y - map.Position.Y);

                tempSelectionBarPosition.X = oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX;
                tempSelectionBarPosition.Y = oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY;

                GameMode = GameModes.TransitionInEnemyAttackMode;
            }

            if (oldSelectionBarPosition != selectionBar.Position)
            {
                tempSelectionBarPosition.X = oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX;
                tempSelectionBarPosition.Y = oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY;

                if (oldSelectionBarPosition.X < selectionBar.Position.X)
                    oldSelectionBarPosition.X++;
                else if (oldSelectionBarPosition.X > selectionBar.Position.X)
                    oldSelectionBarPosition.X--;

                if (oldSelectionBarPosition.Y < selectionBar.Position.Y)
                    oldSelectionBarPosition.Y++;
                else if (oldSelectionBarPosition.Y > selectionBar.Position.Y)
                    oldSelectionBarPosition.Y--;

                GameMode = GameModes.TransitionInEnemyAttackMode;
            }

            if (GameMode != GameModes.TransitionInEnemyAttackMode)
                EnterEnemyBattleMode();
        }

        private void UpdateVariables_TransitionInEnemyAttackMode()
        {
            bool mapOkay = false;

            if (map.Offset.X != 0 || map.Offset.Y != 0)
            {
                if (map.Offset.X < 0)
                    map.Offset.X += 2 * MOVEX;
                else if (map.Offset.X > 0)
                    map.Offset.X -= 2 * MOVEX;

                if (map.Offset.Y < 0)
                    map.Offset.Y += 2 * MOVEY;
                else if (map.Offset.Y > 0)
                    map.Offset.Y -= 2 * MOVEY;
            }
            else
            {
                oldMapPosition = map.Position;
                mapOkay = true;
            }

            if (tempSelectionBarPosition.X != oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX
                || tempSelectionBarPosition.Y != oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
            {
                if (tempSelectionBarPosition.X < oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X += 2 * MOVEX;
                else if (tempSelectionBarPosition.X > oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X -= 2 * MOVEX;

                if (tempSelectionBarPosition.Y < oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y += 2 * MOVEY;
                else if (tempSelectionBarPosition.Y > oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y -= 2 * MOVEY;
            }
            else if (mapOkay)
                GameMode = GameModes.EnemyAttackMode;
        }

        private void UpdateVariables_HealMenuMode()
        {
            if (oldMapPosition != map.Position)
            {
                map.Offset.X = -Map.TILESIZEX * (oldMapPosition.X - map.Position.X);
                map.Offset.Y = -Map.TILESIZEY * (oldMapPosition.Y - map.Position.Y);

                tempSelectionBarPosition.X = oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX;
                tempSelectionBarPosition.Y = oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY;

                GameMode = GameModes.TransitionInHealMenuMode;
            }

            if (oldSelectionBarPosition != selectionBar.Position)
            {
                tempSelectionBarPosition.X = oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX;
                tempSelectionBarPosition.Y = oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY;

                if (oldSelectionBarPosition.X < selectionBar.Position.X)
                    oldSelectionBarPosition.X++;
                else if (oldSelectionBarPosition.X > selectionBar.Position.X)
                    oldSelectionBarPosition.X--;

                if (oldSelectionBarPosition.Y < selectionBar.Position.Y)
                    oldSelectionBarPosition.Y++;
                else if (oldSelectionBarPosition.Y > selectionBar.Position.Y)
                    oldSelectionBarPosition.Y--;

                GameMode = GameModes.TransitionInHealMenuMode;
            }
        }

        private void UpdateVariables_TransitionInHealMenuMode()
        {
            bool mapOkay = false;

            if (map.Offset.X != 0 || map.Offset.Y != 0)
            {
                if (map.Offset.X < 0)
                    map.Offset.X += 2 * MOVEX;
                else if (map.Offset.X > 0)
                    map.Offset.X -= 2 * MOVEX;

                if (map.Offset.Y < 0)
                    map.Offset.Y += 2 * MOVEY;
                else if (map.Offset.Y > 0)
                    map.Offset.Y -= 2 * MOVEY;
            }
            else
            {
                oldMapPosition = map.Position;
                mapOkay = true;
            }

            if (tempSelectionBarPosition.X != oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX
                || tempSelectionBarPosition.Y != oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
            {
                if (tempSelectionBarPosition.X < oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X += 2 * MOVEX;
                else if (tempSelectionBarPosition.X > oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX)
                    tempSelectionBarPosition.X -= 2 * MOVEX;

                if (tempSelectionBarPosition.Y < oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y += 2 * MOVEY;
                else if (tempSelectionBarPosition.Y > oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY)
                    tempSelectionBarPosition.Y -= 2 * MOVEY;
            }
            else if (mapOkay)
                GameMode = GameModes.HealMenuMode;
        }

        public void UpdateVariables_EndTurnOfCharacterAndNextCharacterMode()
        {
            Party currentParty = party;

            switch (characterPointer.BelongsToSide)
            {
                case CharacterPointer.Sides.Player:
                    currentParty = party;
                    break;

                case CharacterPointer.Sides.CPU_Opponents:
                    currentParty = enemies;
                    break;
            }

            switch (endTurnOfCharacterAndNextCharacterModeState)
            {
                // **** we must still test this function. poisoning, silencing, confusing,...

                case EndTurnOfCharacterAndNextCharacterModeStates.Poisoned:
                    endTurnOfCharacterAndNextCharacterModeState++;
                    if (currentParty.Members[currentParty.MemberOnTurn].Poisoned)
                    {
                        textMessage[0] = currentParty.Members[currentParty.MemberOnTurn].Name + " gets damaged";
                        textMessage[1] = "by " + POISON_EFFECT.ToString() + " because of the poison.";
                        textMessageLength = 2;
                        currentParty.Members[currentParty.MemberOnTurn].HitPoints -= POISON_EFFECT;
                        if (currentParty.Members[currentParty.MemberOnTurn].HitPoints <= 0)
                        {
                            currentParty.Members[currentParty.MemberOnTurn].HitPoints = 0;
                            textMessageLength = 4;
                            switch (currentParty.Side)
                            {
                                case CharacterPointer.Sides.Player:
                                    textMessage[2] = currentParty.Members[currentParty.MemberOnTurn].Name + " is";
                                    textMessage[3] = "exhausted...";
                                    break;

                                case CharacterPointer.Sides.CPU_Opponents:
                                    textMessage[2] = currentParty.Members[currentParty.MemberOnTurn].Name + " is";
                                    textMessage[3] = "defeated!";
                                    break;
                            }
                        }
                        returnGameMode = GameModes.EndTurnOfCharacterAndNextCharacterMode;
                        waitForKeyPress = true;
                        moveOut = true;
                        EnterDisplayTextMessageMode();
                    }
                    break;

                case EndTurnOfCharacterAndNextCharacterModeStates.PoisonedDefeated:
                    endTurnOfCharacterAndNextCharacterModeState++;
                    if (currentParty.Members[currentParty.MemberOnTurn].HitPoints == 0)
                    {
                        if (enemies.MustSurviveIsDefeated())
                            for (int i = 0; i < enemies.NumPartyMembers; i++)
                                enemies.Members[i].HitPoints = 0;
                        endTurnOfCharacterAndNextCharacterModeState = EndTurnOfCharacterAndNextCharacterModeStates.NextCharacter;
                        numTargetsAttackedOrHealed = 1;
                        targetsAttackedOrHealed[0].WhichOne = currentParty.MemberOnTurn;
                        if (currentParty == party)
                            targetsAttackedOrHealed[0].BelongsToSide = CharacterPointer.Sides.Player;
                        else if (currentParty == enemies)
                            targetsAttackedOrHealed[0].BelongsToSide = CharacterPointer.Sides.CPU_Opponents;
                        returnGameMode = GameModes.EndTurnOfCharacterAndNextCharacterMode;
                        EnterDisplayDefeatedCharactersMode();
                    }
                    break;

                case EndTurnOfCharacterAndNextCharacterModeStates.Silenced:
                    endTurnOfCharacterAndNextCharacterModeState++;
                    if (currentParty.Members[currentParty.MemberOnTurn].HitPoints == 0)
                    {
                        if (party.IsDefeated())
                            EnterBattleLost1Mode();
                        else if (enemies.IsDefeated())
                            EnterBattleWonMode();
                    }

                    if (currentParty.Members[currentParty.MemberOnTurn].Silenced > 0)
                    {
                        currentParty.Members[currentParty.MemberOnTurn].Silenced--;
                        if (currentParty.Members[currentParty.MemberOnTurn].Silenced == 0)
                        {
                            textMessage[0] = "DISPEL expired.";
                            textMessage[1] = currentParty.Members[currentParty.MemberOnTurn].Name + " is no longer";
                            textMessage[2] = "silenced.";
                            textMessageLength = 3;
                            returnGameMode = GameModes.EndTurnOfCharacterAndNextCharacterMode;
                            waitForKeyPress = true;
                            moveOut = true;
                            EnterDisplayTextMessageMode();
                        }
                    }
                    break;

                case EndTurnOfCharacterAndNextCharacterModeStates.Confused:
                    endTurnOfCharacterAndNextCharacterModeState++;
                    if (currentParty.Members[currentParty.MemberOnTurn].Confused > 0)
                    {
                        currentParty.Members[currentParty.MemberOnTurn].Confused--;
                        if (currentParty.Members[currentParty.MemberOnTurn].Confused == 0)
                        {
                            textMessage[0] = currentParty.Members[currentParty.MemberOnTurn].Name + " is fine.";
                            textMessageLength = 1;
                            returnGameMode = GameModes.EndTurnOfCharacterAndNextCharacterMode;
                            waitForKeyPress = true;
                            moveOut = true;
                            EnterDisplayTextMessageMode();
                        }
                    }
                    break;

                case EndTurnOfCharacterAndNextCharacterModeStates.Boosted:
                    endTurnOfCharacterAndNextCharacterModeState++;
                    if (currentParty.Members[currentParty.MemberOnTurn].Boosted > 0)
                    {
                        currentParty.Members[currentParty.MemberOnTurn].Boosted--;
                        if (currentParty.Members[currentParty.MemberOnTurn].Boosted == 0)
                        {
                            if (currentParty.Members[currentParty.MemberOnTurn].DefenseBoostedBy >= 0)
                                textMessage[0] = "BOOST expired.";
                            else
                                textMessage[0] = "SLOW expired.";
                            currentParty.Members[currentParty.MemberOnTurn].UnBoost();
                            textMessage[1] = "Agility and defense";
                            textMessage[2] = "return to normal.";
                            textMessageLength = 3;
                            returnGameMode = GameModes.EndTurnOfCharacterAndNextCharacterMode;
                            waitForKeyPress = true;
                            moveOut = true;
                            EnterDisplayTextMessageMode();
                        }
                    }
                    break;

                case EndTurnOfCharacterAndNextCharacterModeStates.AttackBoosted:
                    endTurnOfCharacterAndNextCharacterModeState++;
                    if (currentParty.Members[currentParty.MemberOnTurn].AttackBoosted > 0)
                    {
                        currentParty.Members[currentParty.MemberOnTurn].AttackBoosted--;
                        if (currentParty.Members[currentParty.MemberOnTurn].AttackBoosted == 0)
                        {
                            currentParty.Members[currentParty.MemberOnTurn].UnAttackBoost();
                            textMessage[0] = "ATTACK expired.";
                            textMessage[1] = "Attack returns to normal.";
                            textMessageLength = 2;
                            returnGameMode = GameModes.EndTurnOfCharacterAndNextCharacterMode;
                            waitForKeyPress = true;
                            moveOut = true;
                            EnterDisplayTextMessageMode();
                        }
                    }
                    break;

                case EndTurnOfCharacterAndNextCharacterModeStates.NextCharacter:
                    characterPointer = characterPointer.Next;
                    NextCharacter();
                    break;
            }
        }

        private void UpdateVariables_PlayerAutoMoveMode()
        {
            if (party.Members[party.MemberOnTurn].Position != backUpPosition)
            {
                delayPosition = map.CalcPosition(party.Members[party.MemberOnTurn].Position, map.Position);
                EnterPlayerMovingMode();
                switch (map.bestPath[currentStepNumber])
                {
                    case Map.Directions.Up:
                        party.Members[party.MemberOnTurn].Position -= map.Size.X;
                        party.Members[party.MemberOnTurn].LookUp();
                        break;

                    case Map.Directions.Left:
                        party.Members[party.MemberOnTurn].Position -= 1;
                        party.Members[party.MemberOnTurn].LookLeft();
                        break;

                    case Map.Directions.Right:
                        party.Members[party.MemberOnTurn].Position += 1;
                        party.Members[party.MemberOnTurn].LookRight();
                        break;

                    case Map.Directions.Down:
                        party.Members[party.MemberOnTurn].Position += map.Size.X;
                        party.Members[party.MemberOnTurn].LookDown();
                        break;
                }
                currentStepNumber++;
            }
            else if ((battleMenu.CurrentState == BattleMenu.States.AttackSelected1 || battleMenu.CurrentState == BattleMenu.States.AttackSelected2) && !map.IsVisible(targetPosition, map.Position))
            {
                delayPosition = map.CalcPosition(party.Members[party.MemberOnTurn].Position, map.Position);
                EnterPlayerMovingMode();
            }
            else
            {
                switch (battleMenu.CurrentState)
                {
                    case BattleMenu.States.AttackSelected1:
                    case BattleMenu.States.AttackSelected2:
                        EnterPlayerAutoAttackMode();
                        break;

                    case BattleMenu.States.StaySelected1:
                    case BattleMenu.States.StaySelected2:
                        selectionBar.Position = map.CalcPosition(party.Members[party.MemberOnTurn].Position);
                        party.Members[party.MemberOnTurn].LookDown();
                        EnterEndTurnOfCharacterAndNextCharacterMode();
                        break;
                }
            }
        }

        private void UpdateVariables_PlayerAutoAttackMode()
        {
            if (oldMapPosition != map.Position)
            {
                map.Offset.X = -Map.TILESIZEX * (oldMapPosition.X - map.Position.X);
                map.Offset.Y = -Map.TILESIZEY * (oldMapPosition.Y - map.Position.Y);

                tempSelectionBarPosition.X = oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX;
                tempSelectionBarPosition.Y = oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY;

                returnGameMode = GameModes.PlayerAutoAttackMode;
                GameMode = GameModes.TransitionInAttackMenuMode;
            }

            if (oldSelectionBarPosition != selectionBar.Position)
            {
                tempSelectionBarPosition.X = oldSelectionBarPosition.X * Map.TILESIZEX + SelectionBar.OFFSETX;
                tempSelectionBarPosition.Y = oldSelectionBarPosition.Y * Map.TILESIZEY + SelectionBar.OFFSETY;

                if (oldSelectionBarPosition.X < selectionBar.Position.X)
                    oldSelectionBarPosition.X++;
                else if (oldSelectionBarPosition.X > selectionBar.Position.X)
                    oldSelectionBarPosition.X--;

                if (oldSelectionBarPosition.Y < selectionBar.Position.Y)
                    oldSelectionBarPosition.Y++;
                else if (oldSelectionBarPosition.Y > selectionBar.Position.Y)
                    oldSelectionBarPosition.Y--;

                returnGameMode = GameModes.PlayerAutoAttackMode;
                GameMode = GameModes.TransitionInAttackMenuMode;
            }

            if (GameMode != GameModes.TransitionInAttackMenuMode)
                EnterPlayerBattleMode();
        }

        private void UpdateVariables_BattleFieldFadeOutMode()
        {
            fadingState -= fadingSpeed;
            if (fadingState < fadingSpeed)
                ReturnToReturnGameMode();
        }

        private void UpdateVariables_BattleFieldFadeInMode()
        {
            fadingState += fadingSpeed;
            if (fadingState > 255 - fadingSpeed)
                ReturnToReturnGameMode();
        }

        private void UpdateVariables_AfterTitleScreenMode()
        {
            EnterStoryMode();
        }

        private void UpdateInput()
        {
            if (!lockInput)
                switch (GameMode)
                {
                    case GameModes.SelectionBarMode:
                        UpdateInput_SelectionBarMode();
                        break;

                    case GameModes.PlayerMoveMode:
                        UpdateInput_PlayerMoveMode();
                        break;

                    case GameModes.BattleMenuMode:
                        UpdateInput_BattleMenuMode();
                        break;

                    case GameModes.LargeCharacterStatsMode:
                        UpdateInput_LargeCharacterStatsMode();
                        break;

                    case GameModes.SmallCharacterStatsMode:
                        UpdateInput_SmallCharacterStatsMode();
                        break;

                    case GameModes.AttackMenuMode:
                        UpdateInput_AttackMenuMode();
                        break;

                    case GameModes.GeneralMenuMode:
                        UpdateInput_GeneralMenuMode();
                        break;

                    case GameModes.ItemMenuMode:
                        UpdateInput_ItemMenuMode();
                        break;

                    case GameModes.ItemMagicSelectionMenuMode:
                        UpdateInput_ItemMagicSelectionMenuMode();
                        break;

                    case GameModes.DisplayTextMessageMode:
                        UpdateInput_DisplayTextMessageMode();
                        break;

                    case GameModes.DropItemMode:
                        UpdateInput_DropItemMode();
                        break;

                    case GameModes.GiveItemMode:
                        UpdateInput_GiveItemMode();
                        break;

                    case GameModes.SwapItemMode:
                        UpdateInput_SwapItemMode();
                        break;

                    case GameModes.EquipWeaponMode:
                        UpdateInput_EquipWeaponMode();
                        break;

                    case GameModes.EquipRingMode:
                        UpdateInput_EquipRingMode();
                        break;

                    case GameModes.PlayerBattleDisplayTextMessageMode:
                        UpdateInput_PlayerBattleDisplayTextMessageMode();
                        break;

                    case GameModes.MagicLevelSelectionMode:
                        UpdateInput_MagicLevelSelectionMode();
                        break;

                    case GameModes.EnemyBattleDisplayTextMessageMode:
                        UpdateInput_EnemyBattleDisplayTextMessageMode();
                        break;

                    case GameModes.HealMenuMode:
                        UpdateInput_HealMenuMode();
                        break;

                    case GameModes.MemberMenuMode:
                        UpdateInput_MemberMenuMode();
                        break;

                    case GameModes.TitleScreenMode:
                        UpdateInput_TitleScreenMode();
                        break;

                    case GameModes.StoryMode:
                        UpdateInput_StoryMode();
                        break;
                }
        }

        private void UpdateInput_SelectionBarMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Up) && newState.IsKeyDown(Keys.Left))
            {
                if (!oldState.IsKeyDown(Keys.Up) && !oldState.IsKeyDown(Keys.Left))
                {
                    keyHoldCounter = 0;
                    UpdateInput_SelectionBarMode_UpLeft();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_SelectionBarMode_UpLeft();
                }
            }
            else if (newState.IsKeyDown(Keys.Up) && newState.IsKeyDown(Keys.Right))
            {
                if (!oldState.IsKeyDown(Keys.Up) && !oldState.IsKeyDown(Keys.Right))
                {
                    keyHoldCounter = 0;
                    UpdateInput_SelectionBarMode_UpRight();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_SelectionBarMode_UpRight();
                }
            }
            else if (newState.IsKeyDown(Keys.Down) && newState.IsKeyDown(Keys.Left))
            {
                if (!oldState.IsKeyDown(Keys.Down) && !oldState.IsKeyDown(Keys.Left))
                {
                    keyHoldCounter = 0;
                    UpdateInput_SelectionBarMode_DownLeft();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_SelectionBarMode_DownLeft();
                }
            }
            else if (newState.IsKeyDown(Keys.Down) && newState.IsKeyDown(Keys.Right))
            {
                if (!oldState.IsKeyDown(Keys.Down) && !oldState.IsKeyDown(Keys.Right))
                {
                    keyHoldCounter = 0;
                    UpdateInput_SelectionBarMode_DownRight();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_SelectionBarMode_DownRight();
                }
            }
            else if (newState.IsKeyDown(Keys.Left))
            {
                if (!oldState.IsKeyDown(Keys.Left))
                {
                    keyHoldCounter = 0;
                    UpdateInput_SelectionBarMode_Left();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_SelectionBarMode_Left();
                }
            }
            else if (newState.IsKeyDown(Keys.Right))
            {
                if (!oldState.IsKeyDown(Keys.Right))
                {
                    keyHoldCounter = 0;
                    UpdateInput_SelectionBarMode_Right();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_SelectionBarMode_Right();
                }
            }
            else if (newState.IsKeyDown(Keys.Up))
            {
                if (!oldState.IsKeyDown(Keys.Up))
                {
                    keyHoldCounter = 0;
                    UpdateInput_SelectionBarMode_Up();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_SelectionBarMode_Up();
                }
            }
            else if (newState.IsKeyDown(Keys.Down))
            {
                if (!oldState.IsKeyDown(Keys.Down))
                {
                    keyHoldCounter = 0;
                    UpdateInput_SelectionBarMode_Down();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_SelectionBarMode_Down();
                }
            }
            else if (newState.IsKeyDown(Keys.A))
            {
                if (!oldState.IsKeyDown(Keys.A))
                {
                    if (map.AnyCharacterLocatedAt(selectionBar.PositionInMap(map), party))
                    {
                        whoseStatsToBeDisplayed.BelongsToSide = CharacterPointer.Sides.Player;
                        whoseStatsToBeDisplayed.WhichOne = map.CharacterLocatedAt(selectionBar.PositionInMap(map), party);
                        returnGameMode = GameModes.SelectionBarMode;
                        EnterLargeCharacterStatsMode();
                    }
                    else if (map.AnyCharacterLocatedAt(selectionBar.PositionInMap(map), enemies))
                    {
                        whoseStatsToBeDisplayed.BelongsToSide = CharacterPointer.Sides.CPU_Opponents;
                        whoseStatsToBeDisplayed.WhichOne = map.CharacterLocatedAt(selectionBar.PositionInMap(map), enemies);
                        returnGameMode = GameModes.SelectionBarMode;
                        EnterLargeCharacterStatsMode();
                    }
                    else
                        EnterMemberMenuMode();
// Difference to Shining Force 2: in this game, there is no general menu.
//                        EnterGeneralMenuMoveInMode();
                }
            }
            else if (newState.IsKeyDown(Keys.S))
            {
                if (!oldState.IsKeyDown(Keys.S))
                    EnterPlayerMoveTransitionMode();
            }
            else if (newState.IsKeyDown(Keys.D))
            {
                if (!oldState.IsKeyDown(Keys.D))
                {
                    if (selectionBar.Position == map.CalcPosition(party.Members[party.MemberOnTurn].Position))
                        EnterPlayerMoveTransitionMode();
                    else
                    {
                        if (map.AnyCharacterLocatedAt(selectionBar.PositionInMap(map), party))
                        {
                            whoseStatsToBeDisplayed.BelongsToSide = CharacterPointer.Sides.Player;
                            whoseStatsToBeDisplayed.WhichOne = map.CharacterLocatedAt(selectionBar.PositionInMap(map), party);
                            EnterSmallCharacterStatsMode();
                        }
                        else if (map.AnyCharacterLocatedAt(selectionBar.PositionInMap(map), enemies))
                        {
                            whoseStatsToBeDisplayed.BelongsToSide = CharacterPointer.Sides.CPU_Opponents;
                            whoseStatsToBeDisplayed.WhichOne = map.CharacterLocatedAt(selectionBar.PositionInMap(map), enemies);
                            EnterSmallCharacterStatsMode();
                        }
                        else
                            EnterMemberMenuMode();
// Difference to Shining Force 2: in this game, there is no general menu.
//                        EnterGeneralMenuMoveInMode();
                    }
                }
            }

            oldState = newState;
        }

        private void UpdateInput_SelectionBarMode_UpLeft()
        {
            if (!map.AtTopBorder(selectionBar.Position) && !map.AtLeftBorder(selectionBar.Position))
            {
                oldSelectionBarPosition = selectionBar.Position;
                if (map.AtTopBorder(selectionBar.Position - new Vector2(0, 3)))
                {
                    map.Position.Y--;
                    if (!map.InBoundaries(map.Position))
                    {
                        map.Position.Y++;
                        selectionBar.Position.Y--;
                    }
                    else
                        map.Offset.Y = -Map.TILESIZEY;
                }
                else
                    selectionBar.Position.Y--;

                if (map.AtLeftBorder(selectionBar.Position - new Vector2(3, 0)))
                {
                    map.Position.X--;
                    if (!map.InBoundaries(map.Position))
                    {
                        map.Position.X++;
                        selectionBar.Position.X--;
                    }
                    else
                        map.Offset.X = -Map.TILESIZEX;
                }
                else
                    selectionBar.Position.X--;
            }
        }

        private void UpdateInput_SelectionBarMode_UpRight()
        {
            if (!map.AtTopBorder(selectionBar.Position) && !map.AtRightBorder(selectionBar.Position))
            {
                oldSelectionBarPosition = selectionBar.Position;
                if (map.AtTopBorder(selectionBar.Position - new Vector2(0, 3)))
                {
                    map.Position.Y--;
                    if (!map.InBoundaries(map.Position))
                    {
                        map.Position.Y++;
                        selectionBar.Position.Y--;
                    }
                    else
                        map.Offset.Y = -Map.TILESIZEY;
                }
                else
                    selectionBar.Position.Y--;

                if (map.AtRightBorder(selectionBar.Position + new Vector2(3, 0)))
                {
                    map.Position.X++;
                    if (!map.InBoundaries(map.Position))
                    {
                        map.Position.X--;
                        selectionBar.Position.X++;
                    }
                    else
                        map.Offset.X = Map.TILESIZEX;
                }
                else
                    selectionBar.Position.X++;
            }
        }

        private void UpdateInput_SelectionBarMode_DownLeft()
        {
            if (!map.AtBottomBorder(selectionBar.Position) && !map.AtLeftBorder(selectionBar.Position))
            {
                oldSelectionBarPosition = selectionBar.Position;
                if (map.AtBottomBorder(selectionBar.Position + new Vector2(0, 3)))
                {
                    map.Position.Y++;
                    if (!map.InBoundaries(map.Position))
                    {
                        map.Position.Y--;
                        selectionBar.Position.Y++;
                    }
                    else
                        map.Offset.Y = Map.TILESIZEY;
                }
                else
                    selectionBar.Position.Y++;

                if (map.AtLeftBorder(selectionBar.Position - new Vector2(3, 0)))
                {
                    map.Position.X--;
                    if (!map.InBoundaries(map.Position))
                    {
                        map.Position.X++;
                        selectionBar.Position.X--;
                    }
                    else
                        map.Offset.X = -Map.TILESIZEX;
                }
                else
                    selectionBar.Position.X--;
            }
        }

        private void UpdateInput_SelectionBarMode_DownRight()
        {
            if (!map.AtBottomBorder(selectionBar.Position) && !map.AtRightBorder(selectionBar.Position))
            {
                oldSelectionBarPosition = selectionBar.Position;
                if (map.AtBottomBorder(selectionBar.Position + new Vector2(0, 3)))
                {
                    map.Position.Y++;
                    if (!map.InBoundaries(map.Position))
                    {
                        map.Position.Y--;
                        selectionBar.Position.Y++;
                    }
                    else
                        map.Offset.Y = Map.TILESIZEY;
                }
                else
                    selectionBar.Position.Y++;

                if (map.AtRightBorder(selectionBar.Position + new Vector2(3, 0)))
                {
                    map.Position.X++;
                    if (!map.InBoundaries(map.Position))
                    {
                        map.Position.X--;
                        selectionBar.Position.X++;
                    }
                    else
                        map.Offset.X = Map.TILESIZEX;
                }
                else
                    selectionBar.Position.X++;
            }
        }

        private void UpdateInput_SelectionBarMode_Left()
        {
            if (!map.AtLeftBorder(selectionBar.Position))
            {
                oldSelectionBarPosition = selectionBar.Position;
                if (map.AtLeftBorder(selectionBar.Position - new Vector2(3, 0)))
                {
                    map.Position.X--;
                    if (!map.InBoundaries(map.Position))
                    {
                        map.Position.X++;
                        selectionBar.Position.X--;
                    }
                    else
                        map.Offset.X = -Map.TILESIZEX;
                }
                else
                    selectionBar.Position.X--;
            }
        }

        private void UpdateInput_SelectionBarMode_Right()
        {
            if (!map.AtRightBorder(selectionBar.Position))
            {
                oldSelectionBarPosition = selectionBar.Position;
                if (map.AtRightBorder(selectionBar.Position + new Vector2(3, 0)))
                {
                    map.Position.X++;
                    if (!map.InBoundaries(map.Position))
                    {
                        map.Position.X--;
                        selectionBar.Position.X++;
                    }
                    else
                        map.Offset.X = Map.TILESIZEX;
                }
                else
                    selectionBar.Position.X++;
            }
        }

        private void UpdateInput_SelectionBarMode_Up()
        {
            if (!map.AtTopBorder(selectionBar.Position))
            {
                oldSelectionBarPosition = selectionBar.Position;
                if (map.AtTopBorder(selectionBar.Position - new Vector2(0, 3)))
                {
                    map.Position.Y--;
                    if (!map.InBoundaries(map.Position))
                    {
                        map.Position.Y++;
                        selectionBar.Position.Y--;
                    }
                    else
                        map.Offset.Y = -Map.TILESIZEY;
                }
                else
                    selectionBar.Position.Y--;
            }
        }

        private void UpdateInput_SelectionBarMode_Down()
        {
            if (!map.AtBottomBorder(selectionBar.Position))
            {
                oldSelectionBarPosition = selectionBar.Position;
                if (map.AtBottomBorder(selectionBar.Position + new Vector2(0, 3)))
                {
                    map.Position.Y++;
                    if (!map.InBoundaries(map.Position))
                    {
                        map.Position.Y--;
                        selectionBar.Position.Y++;
                    }
                    else
                        map.Offset.Y = Map.TILESIZEY;
                }
                else
                    selectionBar.Position.Y++;
            }
        }

        private void UpdateInput_PlayerMoveMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Left))
                UpdateInput_PlayerMoveMode_Left();
            else if (newState.IsKeyDown(Keys.Right))
                UpdateInput_PlayerMoveMode_Right();
            else if (newState.IsKeyDown(Keys.Up))
                UpdateInput_PlayerMoveMode_Up();
            else if (newState.IsKeyDown(Keys.Down))
                UpdateInput_PlayerMoveMode_Down();
            else if (newState.IsKeyDown(Keys.S))
            {
                if (!oldState.IsKeyDown(Keys.S))
                {
                    LeavePlayerMoveMode();
                    EnterSelectionBarTransitionMode();
                }
            }
            else if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.D))
            {
                if (!oldState.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.D))
                {
                    if (!map.IsOccupied(party.Members[party.MemberOnTurn].Position, characterPointer, party, enemies))
                    {
                        LeavePlayerMoveMode();
                        EnterBattleMenuMoveInMode();
                    }
                    else
                    {
                        // if sound were implemented, here would come a signal that signifies
                        // that this position is already occupied
                    }
                }
            }

            oldState = newState;
        }

        private void UpdateInput_PlayerMoveMode_Left()
        {
            party.Members[party.MemberOnTurn].LookLeft();

            if (map.IsViable(party.Members[party.MemberOnTurn].Position - 1))
            {
                delayPosition = map.CalcPosition(party.Members[party.MemberOnTurn].Position, map.Position);
                if (map.AtLeftBorder(map.CalcPosition(party.Members[party.MemberOnTurn].Position) - new Vector2(3, 0)))
                {
                    oldMapPosition = map.Position;
                    map.Position.X--;
                    if (!map.InBoundaries(map.Position))
                        map.Position.X++;
                    else
                        map.Offset.X = -Map.TILESIZEX;
                }
                EnterPlayerMovingMode();
                party.Members[party.MemberOnTurn].Position--;
            }
        }

        private void UpdateInput_PlayerMoveMode_Right()
        {
            party.Members[party.MemberOnTurn].LookRight();

            if (map.IsViable(party.Members[party.MemberOnTurn].Position + 1))
            {
                delayPosition = map.CalcPosition(party.Members[party.MemberOnTurn].Position, map.Position);
                if (map.AtRightBorder(map.CalcPosition(party.Members[party.MemberOnTurn].Position) + new Vector2(3, 0)))
                {
                    oldMapPosition = map.Position;
                    map.Position.X++;
                    if (!map.InBoundaries(map.Position))
                        map.Position.X--;
                    else
                        map.Offset.X = Map.TILESIZEX;
                }
                EnterPlayerMovingMode();
                party.Members[party.MemberOnTurn].Position++;
            }
        }

        private void UpdateInput_PlayerMoveMode_Up()
        {
            party.Members[party.MemberOnTurn].LookUp();

            if ((party.Members[party.MemberOnTurn].Position - map.Size.X >= 2) && map.IsViable(party.Members[party.MemberOnTurn].Position - map.Size.X))
            {
                delayPosition = map.CalcPosition(party.Members[party.MemberOnTurn].Position, map.Position);
                if (map.AtTopBorder(map.CalcPosition(party.Members[party.MemberOnTurn].Position) - new Vector2(0, 3)))
                {
                    oldMapPosition = map.Position;
                    map.Position.Y--;
                    if (!map.InBoundaries(map.Position))
                        map.Position.Y++;
                    else
                        map.Offset.Y = -Map.TILESIZEY;
                }
                EnterPlayerMovingMode();
                party.Members[party.MemberOnTurn].Position -= map.Size.X;
            }
        }

        private void UpdateInput_PlayerMoveMode_Down()
        {
            party.Members[party.MemberOnTurn].LookDown();

            if ((party.Members[party.MemberOnTurn].Position + map.Size.X <= 1 + map.Size.X * map.Size.Y) && map.IsViable(party.Members[party.MemberOnTurn].Position + map.Size.X))
            {
                delayPosition = map.CalcPosition(party.Members[party.MemberOnTurn].Position, map.Position);
                if (map.AtBottomBorder(map.CalcPosition(party.Members[party.MemberOnTurn].Position) + new Vector2(0, 3)))
                {
                    oldMapPosition = map.Position;
                    map.Position.Y++;
                    if (!map.InBoundaries(map.Position))
                        map.Position.Y--;
                    else
                        map.Offset.Y = Map.TILESIZEY;
                }
                EnterPlayerMovingMode();
                party.Members[party.MemberOnTurn].Position += map.Size.X;
            }
        }

        private void UpdateInput_BattleMenuMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Left))
            {
                if (!oldState.IsKeyDown(Keys.Left))
                {
                    keyHoldCounter = 0;
                    battleMenu.CurrentState = BattleMenu.States.MagicSelected2;
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        battleMenu.CurrentState = BattleMenu.States.MagicSelected2;
                }
            }
            else if (newState.IsKeyDown(Keys.Right))
            {
                if (!oldState.IsKeyDown(Keys.Right))
                {
                    keyHoldCounter = 0;
                    battleMenu.CurrentState = BattleMenu.States.ItemSelected2;
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        battleMenu.CurrentState = BattleMenu.States.ItemSelected2;
                }
            }
            else if (newState.IsKeyDown(Keys.Up))
            {
                if (!oldState.IsKeyDown(Keys.Up))
                {
                    keyHoldCounter = 0;
                    battleMenu.CurrentState = BattleMenu.States.AttackSelected2;
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        battleMenu.CurrentState = BattleMenu.States.AttackSelected2;
                }
            }
            else if (newState.IsKeyDown(Keys.Down))
            {
                if (!oldState.IsKeyDown(Keys.Down))
                {
                    keyHoldCounter = 0;
                    battleMenu.CurrentState = BattleMenu.States.StaySelected2;
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        battleMenu.CurrentState = BattleMenu.States.StaySelected2;
                }
            }
            else if (newState.IsKeyDown(Keys.S))
            {
                if (!oldState.IsKeyDown(Keys.S))
                    LeaveBattleMenuMode();
            }
            else if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.D))
            {
                if (!oldState.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.D))
                    switch (battleMenu.CurrentState)
                    {
                        case BattleMenu.States.StaySelected1:
                        case BattleMenu.States.StaySelected2:
                            selectionBar.Position = map.CalcPosition(party.Members[party.MemberOnTurn].Position);

                            party.Members[party.MemberOnTurn].LookDown();
                            EnterEndTurnOfCharacterAndNextCharacterMode();
                            break;

                        case BattleMenu.States.AttackSelected1:
                        case BattleMenu.States.AttackSelected2:
                            if (map.AnyCharacterLocatedInMarkedFields(enemies))
                                EnterAttackMenuMode();
                            else
                            {
                                textMessage[0] = "No opponent there.";
                                textMessageLength = 1;
                                returnGameMode = GameModes.BattleMenuMode;
                                waitForKeyPress = true;
                                moveOut = true;
                                EnterDisplayTextMessageMode();
                            }
                            break;

                        case BattleMenu.States.ItemSelected1:
                        case BattleMenu.States.ItemSelected2:
                            if (party.Members[party.MemberOnTurn].Items != null)
                                EnterItemMagicSelectionMenuMoveInMode();
                                // EnterItemMenuMode();
                                // In Shining Force 2 the game enters item menu mode at this stage. In this game items can only be used, not given, equipped or discarded.
                            else
                            {
                                textMessage[0] = "You have no item.";
                                textMessageLength = 1;
                                returnGameMode = GameModes.BattleMenuMode;
                                waitForKeyPress = true;
                                moveOut = true;
                                map.EmptyMapMarked();
                                EnterDisplayTextMessageMode();
                            }
                            break;

                        case BattleMenu.States.MagicSelected1:
                        case BattleMenu.States.MagicSelected2:
                            if (party.Members[party.MemberOnTurn].MagicSpells != null)
                                EnterItemMagicSelectionMenuMoveInMode();
                            else
                            {
                                textMessage[0] = "Learned no new magic spell.";
                                textMessageLength = 1;
                                returnGameMode = GameModes.BattleMenuMode;
                                waitForKeyPress = true;
                                moveOut = true;
                                map.EmptyMapMarked();
                                EnterDisplayTextMessageMode();
                            }
                            break;
                    }
            }

            oldState = newState;
        }

        private void UpdateInput_LargeCharacterStatsMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.S) || newState.IsKeyDown(Keys.D))
                if (!oldState.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.S) && !oldState.IsKeyDown(Keys.D))
                {
                    if (returnGameMode == GameModes.SelectionBarMode
                        && whoseStatsToBeDisplayed.BelongsToSide == CharacterPointer.Sides.Player 
                        && characterPointer.BelongsToSide == CharacterPointer.Sides.Player 
                        && whoseStatsToBeDisplayed.WhichOne == party.MemberOnTurn)
                        EnterPlayerMoveTransitionMode();
                    else
                        ReturnToReturnGameMode();
                }

            oldState = newState;
        }

        private void UpdateInput_SmallCharacterStatsMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.S) || newState.IsKeyDown(Keys.D) || newState.IsKeyDown(Keys.Up) || newState.IsKeyDown(Keys.Left) || newState.IsKeyDown(Keys.Right) || newState.IsKeyDown(Keys.Down))
                if (!oldState.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.S) && !oldState.IsKeyDown(Keys.D) && !oldState.IsKeyDown(Keys.Up) && !oldState.IsKeyDown(Keys.Left) && !oldState.IsKeyDown(Keys.Right) && !oldState.IsKeyDown(Keys.Down))
                    GameMode = GameModes.SelectionBarMode;

            oldState = newState;
        }

        private void UpdateInput_AttackMenuMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Right) || newState.IsKeyDown(Keys.Down))
            {
                if (!oldState.IsKeyDown(Keys.Right) && !oldState.IsKeyDown(Keys.Down))
                {
                    keyHoldCounter = 0;
                    UpdateInput_AttackMenuMode_CursorKey1();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_AttackMenuMode_CursorKey1();
                }
            }
            else if (newState.IsKeyDown(Keys.Left) || newState.IsKeyDown(Keys.Up))
            {
                if (!oldState.IsKeyDown(Keys.Left) && !oldState.IsKeyDown(Keys.Up))
                {
                    keyHoldCounter = 0;
                    UpdateInput_AttackMenuMode_CursorKey2();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_AttackMenuMode_CursorKey2();
                }
            }
            else if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.D))
            {
                if (!oldState.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.D))
                    EnterPlayerBattleMode();
            }
            else if (newState.IsKeyDown(Keys.S))
                if (!oldState.IsKeyDown(Keys.S))
                    EnterSelectionBarTransitionOutAttackMenuMode();

            oldState = newState;
        }

        private void UpdateInput_AttackMenuMode_CursorKey1()
        {
            skip++;
            if (map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip) == -1)
                skip = 0;
            UpdateInput_AttackMenuMode_CursorKey_Common();
        }

        private void UpdateInput_AttackMenuMode_CursorKey2()
        {
            if (skip == 0)
                while (map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip) != -1)
                    skip++;

            skip--;
            UpdateInput_AttackMenuMode_CursorKey_Common();
        }

        private void UpdateInput_AttackMenuMode_CursorKey_Common()
        {
            selectionBar.Position = map.CalcPosition(map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip));

            if (map.CalcPosition(party.Members[party.MemberOnTurn].Position).Y < selectionBar.Position.Y)
                party.Members[party.MemberOnTurn].LookDown();
            else if (map.CalcPosition(party.Members[party.MemberOnTurn].Position).Y > selectionBar.Position.Y)
                party.Members[party.MemberOnTurn].LookUp();
            else if (map.CalcPosition(party.Members[party.MemberOnTurn].Position).X < selectionBar.Position.X)
                party.Members[party.MemberOnTurn].LookRight();
            else if (map.CalcPosition(party.Members[party.MemberOnTurn].Position).X > selectionBar.Position.X)
                party.Members[party.MemberOnTurn].LookLeft();

            oldMapPosition = map.Position;

            if (selectionBar.Position.Y < 3)
            {
                map.Position.Y -= 3 - selectionBar.Position.Y;
                selectionBar.Position.Y = 3;
                while (!map.InBoundaries(map.Position))
                {
                    map.Position.Y++;
                    selectionBar.Position.Y--;
                }
            }

            target = new CharacterPointer(CharacterPointer.Sides.CPU_Opponents, map.CharacterLocatedAt(selectionBar.PositionInMap(map), enemies));
        }

        private void UpdateInput_GeneralMenuMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Left))
            {
                if (!oldState.IsKeyDown(Keys.Left))
                {
                    keyHoldCounter = 0;
                    generalMenu.CurrentState = GeneralMenu.States.MapSelected2;
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        generalMenu.CurrentState = GeneralMenu.States.MapSelected2;
                }
            }
            else if (newState.IsKeyDown(Keys.Right))
            {
                if (!oldState.IsKeyDown(Keys.Right))
                {
                    keyHoldCounter = 0;
                    generalMenu.CurrentState = GeneralMenu.States.SpeedSelected2;
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        generalMenu.CurrentState = GeneralMenu.States.SpeedSelected2;
                }
            }
            else if (newState.IsKeyDown(Keys.Up))
            {
                if (!oldState.IsKeyDown(Keys.Up))
                {
                    keyHoldCounter = 0;
                    generalMenu.CurrentState = GeneralMenu.States.MemberSelected2;
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        generalMenu.CurrentState = GeneralMenu.States.MemberSelected2;
                }
            }
            else if (newState.IsKeyDown(Keys.Down))
            {
                if (!oldState.IsKeyDown(Keys.Down))
                {
                    keyHoldCounter = 0;
                    generalMenu.CurrentState = GeneralMenu.States.QuitSelected2;
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        generalMenu.CurrentState = GeneralMenu.States.QuitSelected2;
                }
            }
            else if (newState.IsKeyDown(Keys.S))
            {
                if (!oldState.IsKeyDown(Keys.S))
                    LeaveGeneralMenuMode();
            }
            else if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.D))
            {
                if (!oldState.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.D))
                    switch (generalMenu.CurrentState)
                    {
                        // *** here comes the code for the general menu options
                        case GeneralMenu.States.MemberSelected1:
                        case GeneralMenu.States.MemberSelected2:
                            EnterMemberMenuMode();
                            break;
                    }
            }

            oldState = newState;
        }

        private void UpdateInput_ItemMenuMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Left))
            {
                if (!oldState.IsKeyDown(Keys.Left))
                {
                    keyHoldCounter = 0;
                    itemMenu.CurrentState = ItemMenu.States.GiveSelected2;
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        itemMenu.CurrentState = ItemMenu.States.GiveSelected2;
                }
            }
            else if (newState.IsKeyDown(Keys.Right))
            {
                if (!oldState.IsKeyDown(Keys.Right))
                {
                    keyHoldCounter = 0;
                    itemMenu.CurrentState = ItemMenu.States.EquipSelected2;
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        itemMenu.CurrentState = ItemMenu.States.EquipSelected2;
                }
            }
            else if (newState.IsKeyDown(Keys.Up))
            {
                if (!oldState.IsKeyDown(Keys.Up))
                {
                    keyHoldCounter = 0;
                    itemMenu.CurrentState = ItemMenu.States.UseSelected2;
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        itemMenu.CurrentState = ItemMenu.States.UseSelected2;
                }
            }
            else if (newState.IsKeyDown(Keys.Down))
            {
                if (!oldState.IsKeyDown(Keys.Down))
                {
                    keyHoldCounter = 0;
                    itemMenu.CurrentState = ItemMenu.States.DropSelected2;
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        itemMenu.CurrentState = ItemMenu.States.DropSelected2;
                }
            }
            else if (newState.IsKeyDown(Keys.S))
            {
                if (!oldState.IsKeyDown(Keys.S))
                    EnterItemMenuMoveOutMode();
            }
            else if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.D))
            {
                if (!oldState.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.D))
                {
                    if (itemMenu.CurrentState == ItemMenu.States.GiveSelected1 || itemMenu.CurrentState == ItemMenu.States.GiveSelected2)
                    {
                        map.EmptyMapMarked();
                        map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, 1);
                        if (map.AnyCharacterLocatedInMarkedFields(party))
                            EnterItemMagicSelectionMenuMoveInMode();
                        else
                        {
                            returnGameMode = GameModes.ItemMenuMode;
                            textMessage[0] = "No party member there.";
                            textMessageLength = 1;
                            waitForKeyPress = true;
                            moveOut = true;
                            EnterDisplayTextMessageMode();
                        }
                    }
                    else if (itemMenu.CurrentState == ItemMenu.States.DropSelected1 || itemMenu.CurrentState == ItemMenu.States.DropSelected2 
                             || itemMenu.CurrentState == ItemMenu.States.UseSelected1 || itemMenu.CurrentState == ItemMenu.States.UseSelected2)
                        EnterItemMagicSelectionMenuMoveInMode();
                    else if (itemMenu.CurrentState == ItemMenu.States.EquipSelected1 || itemMenu.CurrentState == ItemMenu.States.EquipSelected2)
                    {
                        if (party.Members[party.MemberOnTurn].GetNextEquippableWeapon(0) != -1)
                            EnterEquipWeaponMoveInMode();
                        else if (party.Members[party.MemberOnTurn].GetNextEquippableRing(0) != -1)
                            EnterEquipRingMoveInMode();
                        else
                        {
                            returnGameMode = GameModes.ItemMenuMode;
                            textMessage[0] = "You have nothing to equip.";
                            textMessageLength = 1;
                            waitForKeyPress = true;
                            moveOut = true;
                            EnterDisplayTextMessageMode();
                        }
                    }
                }
            }

            oldState = newState;
        }

        private void UpdateInput_ItemMagicSelectionMenuMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Left))
            {
                if (!oldState.IsKeyDown(Keys.Left))
                {
                    keyHoldCounter = 0;
                    UpdateInput_ItemMagicSelectionMenuMode_Left();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_ItemMagicSelectionMenuMode_Left();
                }
            }
            else if (newState.IsKeyDown(Keys.Right))
            {
                if (!oldState.IsKeyDown(Keys.Right))
                {
                    keyHoldCounter = 0;
                    UpdateInput_ItemMagicSelectionMenuMode_Right();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_ItemMagicSelectionMenuMode_Right();
                }
            }
            else if (newState.IsKeyDown(Keys.Up))
            {
                if (!oldState.IsKeyDown(Keys.Up))
                {
                    keyHoldCounter = 0;
                    UpdateInput_ItemMagicSelectionMenuMode_Top();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_ItemMagicSelectionMenuMode_Top();
                }
            }
            else if (newState.IsKeyDown(Keys.Down))
            {
                if (!oldState.IsKeyDown(Keys.Down))
                {
                    keyHoldCounter = 0;
                    UpdateInput_ItemMagicSelectionMenuMode_Bottom();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_ItemMagicSelectionMenuMode_Bottom();
                }
            }
            else if (newState.IsKeyDown(Keys.S))
            {
                if (!oldState.IsKeyDown(Keys.S))
                    EnterItemMagicSelectionMenuMoveOutMode();
            }
            else if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.D))
            {
                if (!oldState.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.D))
                {
                    if (battleMenu.CurrentState == BattleMenu.States.ItemSelected1 || battleMenu.CurrentState == BattleMenu.States.ItemSelected2)
                    {
                        switch (itemMenu.CurrentState)
                        {
                            case ItemMenu.States.UseSelected1:
                            case ItemMenu.States.UseSelected2:
                                UseItem();
                                break;

                            case ItemMenu.States.DropSelected1:
                            case ItemMenu.States.DropSelected2:
                                EnterDropItemMode();
                                break;

                            case ItemMenu.States.GiveSelected1:
                            case ItemMenu.States.GiveSelected2:
                                oldSelectionBarPosition = map.CalcPosition(party.Members[party.MemberOnTurn].Position);
                                EnterGiveItemMode();
                                break;
                        }
                    }
                    else
                    {
                        EnterMagicLevelSelectionMode();
                    }
                }
            }

            oldState = newState;
        }

        private void UpdateInput_ItemMagicSelectionMenuMode_Top()
        {
            if (battleMenu.CurrentState == BattleMenu.States.ItemSelected1 || battleMenu.CurrentState == BattleMenu.States.ItemSelected2)
            {
                if (party.Members[party.MemberOnTurn].Items[0] != null)
                    itemMagicSelectionMenu.CurrentState = ItemMagicSelectionMenu.States.TopSelected2;
                selectedItemNumber = 0;

                if (party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell != null)
                {
                    map.EmptyMapMarked();
                    for (int i = party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]; i <= party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]; i++)
                        map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);
                }
            }
            else if (battleMenu.CurrentState == BattleMenu.States.MagicSelected1 || battleMenu.CurrentState == BattleMenu.States.MagicSelected2)
            {
                if (party.Members[party.MemberOnTurn].MagicSpells[0] != null)
                {
                    itemMagicSelectionMenu.CurrentState = ItemMagicSelectionMenu.States.TopSelected2;
                    selectedSpellNumber = 0;
                    map.EmptyMapMarked();
                    for (int i = party.Members[party.MemberOnTurn].MagicSpells[0].MinRange[party.Members[party.MemberOnTurn].MagicSpells[0].Level - 1]; i <= party.Members[party.MemberOnTurn].MagicSpells[0].MaxRange[party.Members[party.MemberOnTurn].MagicSpells[0].Level - 1]; i++)
                        map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);
                }
            }
        }

        private void UpdateInput_ItemMagicSelectionMenuMode_Left()
        {
            if (battleMenu.CurrentState == BattleMenu.States.ItemSelected1 || battleMenu.CurrentState == BattleMenu.States.ItemSelected2)
            {
                if (party.Members[party.MemberOnTurn].Items[1] != null)
                {
                    itemMagicSelectionMenu.CurrentState = ItemMagicSelectionMenu.States.LeftSelected2;
                    selectedItemNumber = 1;

                    if (party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell != null)
                    {
                        map.EmptyMapMarked();
                        for (int i = party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]; i <= party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]; i++)
                            map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);
                    }
                }
            }
            else if (battleMenu.CurrentState == BattleMenu.States.MagicSelected1 || battleMenu.CurrentState == BattleMenu.States.MagicSelected2)
            {
                if (party.Members[party.MemberOnTurn].MagicSpells[1] != null)
                {
                    itemMagicSelectionMenu.CurrentState = ItemMagicSelectionMenu.States.LeftSelected2;
                    selectedSpellNumber = 1;
                    map.EmptyMapMarked();
                    for (int i = party.Members[party.MemberOnTurn].MagicSpells[1].MinRange[party.Members[party.MemberOnTurn].MagicSpells[1].Level - 1]; i <= party.Members[party.MemberOnTurn].MagicSpells[1].MaxRange[party.Members[party.MemberOnTurn].MagicSpells[1].Level - 1]; i++)
                        map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);
                }
            }
        }

        private void UpdateInput_ItemMagicSelectionMenuMode_Right()
        {
            if (battleMenu.CurrentState == BattleMenu.States.ItemSelected1 || battleMenu.CurrentState == BattleMenu.States.ItemSelected2)
            {
                if (party.Members[party.MemberOnTurn].Items[2] != null)
                {
                    itemMagicSelectionMenu.CurrentState = ItemMagicSelectionMenu.States.RightSelected2;
                    selectedItemNumber = 2;

                    if (party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell != null)
                    {
                        map.EmptyMapMarked();
                        for (int i = party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]; i <= party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]; i++)
                            map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);
                    }
                }
            }
            else if (battleMenu.CurrentState == BattleMenu.States.MagicSelected1 || battleMenu.CurrentState == BattleMenu.States.MagicSelected2)
            {
                if (party.Members[party.MemberOnTurn].MagicSpells[2] != null)
                {
                    itemMagicSelectionMenu.CurrentState = ItemMagicSelectionMenu.States.RightSelected2;
                    selectedSpellNumber = 2;
                    map.EmptyMapMarked();
                    for (int i = party.Members[party.MemberOnTurn].MagicSpells[2].MinRange[party.Members[party.MemberOnTurn].MagicSpells[2].Level - 1]; i <= party.Members[party.MemberOnTurn].MagicSpells[2].MaxRange[party.Members[party.MemberOnTurn].MagicSpells[2].Level - 1]; i++)
                        map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);
                }
            }
        }

        private void UpdateInput_ItemMagicSelectionMenuMode_Bottom()
        {
            if (battleMenu.CurrentState == BattleMenu.States.ItemSelected1 || battleMenu.CurrentState == BattleMenu.States.ItemSelected2)
            {
                if (party.Members[party.MemberOnTurn].Items[3] != null)
                {
                    itemMagicSelectionMenu.CurrentState = ItemMagicSelectionMenu.States.BottomSelected2;
                    selectedItemNumber = 3;

                    if (party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell != null)
                    {
                        map.EmptyMapMarked();
                        for (int i = party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]; i <= party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]; i++)
                            map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);
                    }
                }
            }
            else if (battleMenu.CurrentState == BattleMenu.States.MagicSelected1 || battleMenu.CurrentState == BattleMenu.States.MagicSelected2)
            {
                if (party.Members[party.MemberOnTurn].MagicSpells[3] != null)
                {
                    itemMagicSelectionMenu.CurrentState = ItemMagicSelectionMenu.States.BottomSelected2;
                    selectedSpellNumber = 3;
                    map.EmptyMapMarked();
                    for (int i = party.Members[party.MemberOnTurn].MagicSpells[3].MinRange[party.Members[party.MemberOnTurn].MagicSpells[3].Level - 1]; i <= party.Members[party.MemberOnTurn].MagicSpells[3].MaxRange[party.Members[party.MemberOnTurn].MagicSpells[3].Level - 1]; i++)
                        map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);
                }
            }
        }

        private void UpdateInput_DisplayTextMessageMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.S) || newState.IsKeyDown(Keys.D) || newState.IsKeyDown(Keys.Up) || newState.IsKeyDown(Keys.Left) || newState.IsKeyDown(Keys.Right) || newState.IsKeyDown(Keys.Down))
            {
                //if (keyRelieved)
                    textMessageDelay = 6;

                if (!oldState.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.S) && !oldState.IsKeyDown(Keys.D) && !oldState.IsKeyDown(Keys.Up) && !oldState.IsKeyDown(Keys.Left) && !oldState.IsKeyDown(Keys.Right) && !oldState.IsKeyDown(Keys.Down))
                {
                    //keyRelieved = true;

                    if (positionInTextMessage.Column == textMessage[positionInTextMessage.Row].Length - 1)
                    {
                        if (positionInTextMessage.Row == 2 && positionInTextMessage.Row < textMessageLength - 1)
                            textMessageContinueFlag = true;
                        else if (positionInTextMessage.Row == textMessageLength - 1)
                        {
                            if (moveOut)
                                EnterDisplayTextMessageMoveOutMode();
                            else
                                ReturnToReturnGameMode();
                        }
                    }
                }
            }

            oldState = newState;
        }

        private void UpdateInput_DropItemMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Left))
                yesNoButtons.CurrentState = YesNoButtons.States.YesSelected2;
            else if (newState.IsKeyDown(Keys.Right))
                yesNoButtons.CurrentState = YesNoButtons.States.NoSelected2;
            else if (newState.IsKeyDown(Keys.S))
            {
                if (!oldState.IsKeyDown(Keys.S))
                    EnterBattleMenuMode();
            }
            else if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.D))
            {
                if (!oldState.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.D))
                {
                    if (yesNoButtons.CurrentState == YesNoButtons.States.YesSelected1 || yesNoButtons.CurrentState == YesNoButtons.States.YesSelected2)
                    {
                        textMessage[0] = "Discarded the " + GetSelectedItem().Name1 + GetSelectedItem().Name2 + ".";
                        DropItem();
                        textMessageLength = 1;
                        returnGameMode = GameModes.BattleMenuMode;
                        waitForKeyPress = true;
                        moveOut = true;
                        EnterDisplayTextMessageMode();
                    }
                    else
                        EnterBattleMenuMode();
                }
            }

            oldState = newState;
        }

        private void UpdateInput_GiveItemMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Right) || newState.IsKeyDown(Keys.Down))
            {
                if (!oldState.IsKeyDown(Keys.Right) && !oldState.IsKeyDown(Keys.Down))
                {
                    keyHoldCounter = 0;
                    UpdateInput_GiveItemMode_CursorKey1();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_GiveItemMode_CursorKey1();
                }
            }
            else if (newState.IsKeyDown(Keys.Left) || newState.IsKeyDown(Keys.Up))
            {
                if (!oldState.IsKeyDown(Keys.Left) && !oldState.IsKeyDown(Keys.Up))
                {
                    keyHoldCounter = 0;
                    UpdateInput_GiveItemMode_CursorKey2();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_GiveItemMode_CursorKey2();
                }
            }
            else if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.D))
            {
                if (!oldState.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.D))
                    GiveItem();
            }
            else if (newState.IsKeyDown(Keys.S))
                if (!oldState.IsKeyDown(Keys.S))
                    EnterSelectionBarTransitionOutGiveItemMode();

            oldState = newState;
        }

        private void UpdateInput_GiveItemMode_CursorKey1()
        {
            skip++;
            if (map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) == -1)
                skip = 0;
            UpdateInput_GiveItemMode_CursorKey_Common();
        }

        private void UpdateInput_GiveItemMode_CursorKey2()
        {
            if (skip == 0)
                while (map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                    skip++;

            skip--;
            UpdateInput_GiveItemMode_CursorKey_Common();
        }

        private void UpdateInput_GiveItemMode_CursorKey_Common()
        {
            selectionBar.Position = map.CalcPosition(map.GetNextCharacterPositionLocatedInMarkedFields(party, skip));

            if (map.CalcPosition(party.Members[party.MemberOnTurn].Position).Y < selectionBar.Position.Y)
                party.Members[party.MemberOnTurn].LookDown();
            else if (map.CalcPosition(party.Members[party.MemberOnTurn].Position).Y > selectionBar.Position.Y)
                party.Members[party.MemberOnTurn].LookUp();
            else if (map.CalcPosition(party.Members[party.MemberOnTurn].Position).X < selectionBar.Position.X)
                party.Members[party.MemberOnTurn].LookRight();
            else if (map.CalcPosition(party.Members[party.MemberOnTurn].Position).X > selectionBar.Position.X)
                party.Members[party.MemberOnTurn].LookLeft();
        }

        private void UpdateInput_SwapItemMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Left))
            {
                if (!oldState.IsKeyDown(Keys.Left))
                {
                    keyHoldCounter = 0;
                    UpdateInput_SwapItemMode_Left();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_SwapItemMode_Left();
                }
            }
            else if (newState.IsKeyDown(Keys.Right))
            {
                if (!oldState.IsKeyDown(Keys.Right))
                {
                    keyHoldCounter = 0;
                    UpdateInput_SwapItemMode_Right();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_SwapItemMode_Right();
                }
            }
            else if (newState.IsKeyDown(Keys.Up))
            {
                if (!oldState.IsKeyDown(Keys.Up))
                {
                    keyHoldCounter = 0;
                    UpdateInput_SwapItemMode_Top();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_SwapItemMode_Top();
                }
            }
            else if (newState.IsKeyDown(Keys.Down))
            {
                if (!oldState.IsKeyDown(Keys.Down))
                {
                    keyHoldCounter = 0;
                    UpdateInput_SwapItemMode_Bottom();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_SwapItemMode_Bottom();
                }
            }
            else if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.D))
            {
                if (!oldState.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.D))
                    SwapItems();
            }
            else if (newState.IsKeyDown(Keys.S))
                if (!oldState.IsKeyDown(Keys.S))
                    EnterGiveItemMode();

            oldState = newState;
        }

        private void UpdateInput_SwapItemMode_Top()
        {
            if (party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].Items[0] != null)
            {
                swapMenu.CurrentState = ItemMagicSelectionMenu.States.TopSelected2;
                selectedSwapItemNumber = 0;
            }
        }

        private void UpdateInput_SwapItemMode_Left()
        {
            if (party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].Items[1] != null)
            {
                swapMenu.CurrentState = ItemMagicSelectionMenu.States.LeftSelected2;
                selectedSwapItemNumber = 1;
            }
        }

        private void UpdateInput_SwapItemMode_Right()
        {
            if (party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].Items[2] != null)
            {
                swapMenu.CurrentState = ItemMagicSelectionMenu.States.RightSelected2;
                selectedSwapItemNumber = 2;
            }
        }

        private void UpdateInput_SwapItemMode_Bottom()
        {
            if (party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].Items[3] != null)
            {
                swapMenu.CurrentState = ItemMagicSelectionMenu.States.BottomSelected2;
                selectedSwapItemNumber = 3;
            }
        }

        private void UpdateInput_EquipWeaponMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Left))
            {
                if (!oldState.IsKeyDown(Keys.Left))
                {
                    keyHoldCounter = 0;
                    UpdateInput_EquipWeaponMode_Left();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_EquipWeaponMode_Left();
                }
            }
            else if (newState.IsKeyDown(Keys.Right))
            {
                if (!oldState.IsKeyDown(Keys.Right))
                {
                    keyHoldCounter = 0;
                    UpdateInput_EquipWeaponMode_Right();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_EquipWeaponMode_Right();
                }
            }
            else if (newState.IsKeyDown(Keys.Up))
            {
                if (!oldState.IsKeyDown(Keys.Up))
                {
                    keyHoldCounter = 0;
                    UpdateInput_EquipWeaponMode_Top();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_EquipWeaponMode_Top();
                }
            }
            else if (newState.IsKeyDown(Keys.Down))
            {
                if (!oldState.IsKeyDown(Keys.Down))
                {
                    keyHoldCounter = 0;
                    UpdateInput_EquipWeaponMode_Bottom();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_EquipWeaponMode_Bottom();
                }
            }
            else if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.D))
            {
                if (!oldState.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.D))
                {
                    if (party.Members[party.MemberOnTurn].GetNextEquippableRing(0) != -1)
                        EnterEquipRingMoveInMode();
                    else
                        EnterBattleMenuMode();
                }
            }
            else if (newState.IsKeyDown(Keys.S))
                if (!oldState.IsKeyDown(Keys.S))
                {
                    party.Members[party.MemberOnTurn].Unequip(party.Members[party.MemberOnTurn].GetEquippedWeapon());
                    party.Members[party.MemberOnTurn].Equip(backUpEquippedWeapon);
                    EnterItemMagicSelectionMenuMoveInMode();
                    // EnterItemMenuMode();
                    // In Shining Force 2 the game enters item menu mode at this stage. In this game items can only be used, not given, equipped or discarded.
                }

            oldState = newState;
        }

        private void UpdateInput_EquipWeaponMode_Top()
        {
            if (weapons[0] != -1)
            {
                equipWeaponMenu.CurrentState = ItemMagicSelectionMenu.States.TopSelected2;
                selectedWeaponNumber = 0;
                UpdateInput_EquipWeaponMode_Common();
            }
        }

        private void UpdateInput_EquipWeaponMode_Left()
        {
            if (weapons[1] != -1)
            {
                equipWeaponMenu.CurrentState = ItemMagicSelectionMenu.States.LeftSelected2;
                selectedWeaponNumber = 1;
                UpdateInput_EquipWeaponMode_Common();
            }
        }

        private void UpdateInput_EquipWeaponMode_Right()
        {
            if (weapons[2] != -1)
            {
                equipWeaponMenu.CurrentState = ItemMagicSelectionMenu.States.RightSelected2;
                selectedWeaponNumber = 2;
                UpdateInput_EquipWeaponMode_Common();
            }
        }

        private void UpdateInput_EquipWeaponMode_Bottom()
        {
            equipWeaponMenu.CurrentState = ItemMagicSelectionMenu.States.BottomSelected2;
            selectedWeaponNumber = 3;
            UpdateInput_EquipWeaponMode_Common();
        }

        private void UpdateInput_EquipWeaponMode_Common()
        {
            party.Members[party.MemberOnTurn].Unequip(party.Members[party.MemberOnTurn].GetEquippedWeapon());
            party.Members[party.MemberOnTurn].Equip(weapons[selectedWeaponNumber]);
            map.EmptyMapMarked();
            for (int i = party.Members[party.MemberOnTurn].MinAttackRange; i <= party.Members[party.MemberOnTurn].MaxAttackRange; i++)
                map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);
        }

        private void UpdateInput_EquipRingMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Left))
            {
                if (!oldState.IsKeyDown(Keys.Left))
                {
                    keyHoldCounter = 0;
                    UpdateInput_EquipRingMode_Left();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_EquipRingMode_Left();
                }
            }
            else if (newState.IsKeyDown(Keys.Right))
            {
                if (!oldState.IsKeyDown(Keys.Right))
                {
                    keyHoldCounter = 0;
                    UpdateInput_EquipRingMode_Right();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_EquipRingMode_Right();
                }
            }
            else if (newState.IsKeyDown(Keys.Up))
            {
                if (!oldState.IsKeyDown(Keys.Up))
                {
                    keyHoldCounter = 0;
                    UpdateInput_EquipRingMode_Top();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_EquipRingMode_Top();
                }
            }
            else if (newState.IsKeyDown(Keys.Down))
            {
                if (!oldState.IsKeyDown(Keys.Down))
                {
                    keyHoldCounter = 0;
                    UpdateInput_EquipRingMode_Bottom();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_EquipRingMode_Bottom();
                }
            }
            else if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.D))
            {
                if (!oldState.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.D))
                {
                    EnterBattleMenuMoveInMode();
                }
            }
            else if (newState.IsKeyDown(Keys.S))
                if (!oldState.IsKeyDown(Keys.S))
                {
                    party.Members[party.MemberOnTurn].Unequip(party.Members[party.MemberOnTurn].GetEquippedRing());
                    party.Members[party.MemberOnTurn].Equip(backUpEquippedRing);
                    EnterEquipWeaponMode();
                }

            oldState = newState;
        }

        private void UpdateInput_EquipRingMode_Top()
        {
            if (rings[0] != -1)
            {
                equipRingMenu.CurrentState = ItemMagicSelectionMenu.States.TopSelected2;
                selectedRingNumber = 0;
                UpdateInput_EquipRingMode_Common();
            }
        }

        private void UpdateInput_EquipRingMode_Left()
        {
            if (rings[1] != -1)
            {
                equipRingMenu.CurrentState = ItemMagicSelectionMenu.States.LeftSelected2;
                selectedRingNumber = 1;
                UpdateInput_EquipRingMode_Common();
            }
        }

        private void UpdateInput_EquipRingMode_Right()
        {
            if (rings[2] != -1)
            {
                equipRingMenu.CurrentState = ItemMagicSelectionMenu.States.RightSelected2;
                selectedRingNumber = 2;
                UpdateInput_EquipRingMode_Common();
            }
        }

        private void UpdateInput_EquipRingMode_Bottom()
        {
            equipRingMenu.CurrentState = ItemMagicSelectionMenu.States.BottomSelected2;
            selectedRingNumber = 3;
            UpdateInput_EquipRingMode_Common();
        }

        private void UpdateInput_EquipRingMode_Common()
        {
            party.Members[party.MemberOnTurn].Unequip(party.Members[party.MemberOnTurn].GetEquippedRing());
            party.Members[party.MemberOnTurn].Equip(rings[selectedRingNumber]);
        }

        private void UpdateInput_PlayerBattleDisplayTextMessageMode()
        {
            UpdateInput_DisplayTextMessageMode();
        }

        private void UpdateInput_MagicLevelSelectionMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Left))
            {
                if (!oldState.IsKeyDown(Keys.Left))
                {
                    keyHoldCounter = 0;
                    UpdateInput_MagicLevelSelectionMode_Left();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_MagicLevelSelectionMode_Left();
                }
            }
            else if (newState.IsKeyDown(Keys.Right))
            {
                if (!oldState.IsKeyDown(Keys.Right))
                {
                    keyHoldCounter = 0;
                    UpdateInput_MagicLevelSelectionMode_Right();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_MagicLevelSelectionMode_Right();
                }
            }
            else if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.D))
            {
                if (!oldState.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.D))
                {
                    if (party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].MagicPoints[selectedMagicLevel - 1] > party.Members[party.MemberOnTurn].MagicPoints)
                    {
                        textMessage[0] = "More MP needed.";
                        textMessageLength = 1;
                        returnGameMode = GameModes.ItemMagicSelectionMenuMode;
                        waitForKeyPress = true;
                        moveOut = true;
                        EnterDisplayTextMessageMode();
                    }
                    else
                    {
                        switch (party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].Type)
                        {
                            case Spell.Types.Attack:
                                if (map.AnyCharacterLocatedInMarkedFields(enemies))
                                    EnterAttackMenuMode();
                                else
                                {
                                    textMessage[0] = "No opponent there.";
                                    textMessageLength = 1;
                                    returnGameMode = GameModes.ItemMagicSelectionMenuMode;
                                    waitForKeyPress = true;
                                    moveOut = true;
                                    EnterDisplayTextMessageMode();
                                }
                                break;

                            case Spell.Types.Heal:
                            case Spell.Types.Other:
                                if (map.AnyCharacterLocatedInMarkedFields(party))
                                    EnterHealMenuMode();
                                else
                                {
                                    textMessage[0] = "No party member there.";
                                    textMessageLength = 1;
                                    returnGameMode = GameModes.ItemMagicSelectionMenuMode;
                                    waitForKeyPress = true;
                                    moveOut = true;
                                    EnterDisplayTextMessageMode();
                                }
                                break;
                        }
                    }
                }
            }
            else if (newState.IsKeyDown(Keys.S))
                if (!oldState.IsKeyDown(Keys.S))
                {
                    EnterItemMagicSelectionMenuMode();
                }

            oldState = newState;
        }

        private void UpdateInput_MagicLevelSelectionMode_Left()
        {
            selectedMagicLevel--;
            if (selectedMagicLevel == 0)
                selectedMagicLevel = party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].Level;
            map.EmptyMapMarked();
            for (int i = party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1]; i <= party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]; i++)
                map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);
        }

        private void UpdateInput_MagicLevelSelectionMode_Right()
        {
            selectedMagicLevel++;
            if (selectedMagicLevel > party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].Level)
                selectedMagicLevel = 1;
            map.EmptyMapMarked();
            for (int i = party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1]; i <= party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]; i++)
                map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);
        }

        private void UpdateInput_EnemyBattleDisplayTextMessageMode()
        {
            UpdateInput_DisplayTextMessageMode();
        }

        private void UpdateInput_HealMenuMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Right) || newState.IsKeyDown(Keys.Down))
            {
                if (!oldState.IsKeyDown(Keys.Right) && !oldState.IsKeyDown(Keys.Down))
                {
                    keyHoldCounter = 0;
                    UpdateInput_HealMenuMode_CursorKey1();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_HealMenuMode_CursorKey1();
                }
            }
            else if (newState.IsKeyDown(Keys.Left) || newState.IsKeyDown(Keys.Up))
            {
                if (!oldState.IsKeyDown(Keys.Left) && !oldState.IsKeyDown(Keys.Up))
                {
                    keyHoldCounter = 0;
                    UpdateInput_HealMenuMode_CursorKey2();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_HealMenuMode_CursorKey2();
                }
            }
            else if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.D))
            {
                if (!oldState.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.D))
                    if ((battleMenu.CurrentState == BattleMenu.States.MagicSelected1 || battleMenu.CurrentState == BattleMenu.States.MagicSelected2)
                         && party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].Name == "EGRESS")
                    {
                        textMessage[0] = party.Members[party.MemberOnTurn].Name + " cast";
                        textMessage[1] = party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].Name + " level " + selectedMagicLevel.ToString() + "!";
                        textMessageLength = 2;
                        returnGameMode = GameModes.EgressMode;
                        waitForKeyPress = true;
                        moveOut = true;
                        EnterPlayerBattleDisplayTextMessageMode();
                    }
                    else if ((battleMenu.CurrentState == BattleMenu.States.ItemSelected1 || battleMenu.CurrentState == BattleMenu.States.ItemSelected2)
                             && party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Name == "EGRESS")
                    {
                        textMessage[0] = party.Members[party.MemberOnTurn].Name + " uses";
                        textMessage[1] = party.Members[party.MemberOnTurn].Items[selectedItemNumber].Name1 + party.Members[party.MemberOnTurn].Items[selectedItemNumber].Name2 + "!";
                        textMessageLength = 2;
                        returnGameMode = GameModes.EgressMode;
                        waitForKeyPress = true;
                        moveOut = true;
                        EnterPlayerBattleDisplayTextMessageMode();
                    }
                    else
                        EnterPlayerBattleMode();
            }
            else if (newState.IsKeyDown(Keys.S))
                if (!oldState.IsKeyDown(Keys.S))
                    EnterSelectionBarTransitionOutAttackMenuMode(); // we can use this mode

            oldState = newState;
        }

        private void UpdateInput_HealMenuMode_CursorKey1()
        {
            skip++;
            if (map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) == -1)
                skip = 0;
            UpdateInput_HealMenuMode_CursorKey_Common();
        }

        private void UpdateInput_HealMenuMode_CursorKey2()
        {
            if (skip == 0)
                while (map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                    skip++;

            skip--;
            UpdateInput_HealMenuMode_CursorKey_Common();
        }

        private void UpdateInput_HealMenuMode_CursorKey_Common()
        {
            selectionBar.Position = map.CalcPosition(map.GetNextCharacterPositionLocatedInMarkedFields(party, skip));

            if (map.CalcPosition(party.Members[party.MemberOnTurn].Position).Y <= selectionBar.Position.Y)
                party.Members[party.MemberOnTurn].LookDown();
            else if (map.CalcPosition(party.Members[party.MemberOnTurn].Position).Y > selectionBar.Position.Y)
                party.Members[party.MemberOnTurn].LookUp();
            else if (map.CalcPosition(party.Members[party.MemberOnTurn].Position).X < selectionBar.Position.X)
                party.Members[party.MemberOnTurn].LookRight();
            else if (map.CalcPosition(party.Members[party.MemberOnTurn].Position).X > selectionBar.Position.X)
                party.Members[party.MemberOnTurn].LookLeft();

            oldMapPosition = map.Position;

            if (selectionBar.Position.Y < 3)
            {
                map.Position.Y -= 3 - selectionBar.Position.Y;
                selectionBar.Position.Y = 3;
                while (!map.InBoundaries(map.Position))
                {
                    map.Position.Y++;
                    selectionBar.Position.Y--;
                }
            }

            target = new CharacterPointer(CharacterPointer.Sides.Player, map.CharacterLocatedAt(selectionBar.PositionInMap(map), party));
        }

        private void UpdateInput_MemberMenuMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Left) || newState.IsKeyDown(Keys.Right))
            {
                if (!oldState.IsKeyDown(Keys.Left) && !oldState.IsKeyDown(Keys.Right))
                {
                    keyHoldCounter = 0;
                    UpdateInput_MemberMenuMode_LeftRight();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_MemberMenuMode_LeftRight();
                }
            }
            else if (newState.IsKeyDown(Keys.Up))
            {
                if (!oldState.IsKeyDown(Keys.Up))
                {
                    keyHoldCounter = 0;
                    UpdateInput_MemberMenuMode_Up();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_MemberMenuMode_Up();
                }
            }
            else if (newState.IsKeyDown(Keys.Down))
            {
                if (!oldState.IsKeyDown(Keys.Down))
                {
                    keyHoldCounter = 0;
                    UpdateInput_MemberMenuMode_Down();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_MemberMenuMode_Down();
                }
            }
            else if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.D))
            {
                if (!oldState.IsKeyDown(Keys.A) && !oldState.IsKeyDown(Keys.D))
                {
                    keyHoldCounter = 0;
                    UpdateInput_MemberMenuMode_AD();
                }
                else
                {
                    keyHoldCounter++;
                    if (keyHoldCounter > KEY_DELAY)
                        UpdateInput_MemberMenuMode_AD();
                }
            }
            else if (newState.IsKeyDown(Keys.S))
            {
                if (!oldState.IsKeyDown(Keys.S))
                {
                    keyHoldCounter = 0;
                    UpdateInput_MemberMenuMode_S();
                }
            }

            oldState = newState;
        }

        private void UpdateInput_MemberMenuMode_LeftRight()
        {
            memberMenuState_displayDetails = !memberMenuState_displayDetails;
        }

        private void UpdateInput_MemberMenuMode_Up()
        {
            if (memberMenuState_selectedMemberNumber == memberMenuState_firstMemberNumberToDisplay)
            {
                if (memberMenuState_firstMemberNumberToDisplay > 0)
                {
                    memberMenuState_firstMemberNumberToDisplay--;
                    memberMenuState_selectedMemberNumber--;
                }
            }
            else if (memberMenuState_selectedMemberNumber > 0)
                memberMenuState_selectedMemberNumber--;
        }

        private void UpdateInput_MemberMenuMode_Down()
        {
            if (memberMenuState_selectedMemberNumber == memberMenuState_firstMemberNumberToDisplay + MEMBERMENU_NUMCHARSPERPAGE - 1)
            {
                if (memberMenuState_firstMemberNumberToDisplay + MEMBERMENU_NUMCHARSPERPAGE - 1 < party.NumPartyMembers - 1)
                {
                    memberMenuState_firstMemberNumberToDisplay++;
                    memberMenuState_selectedMemberNumber++;
                }
            }
            else if (memberMenuState_selectedMemberNumber < party.NumPartyMembers - 1)
                memberMenuState_selectedMemberNumber++;
        }

        private void UpdateInput_MemberMenuMode_AD()
        {
            whoseStatsToBeDisplayed.BelongsToSide = CharacterPointer.Sides.Player;
            whoseStatsToBeDisplayed.WhichOne = memberMenuState_selectedMemberNumber;
            returnGameMode = GameModes.MemberMenuMode;
            EnterLargeCharacterStatsMode();
        }

        private void UpdateInput_MemberMenuMode_S()
        {
            GameMode = GameModes.SelectionBarMode;
// Difference to Shining Force 2: in this game, there is no general menu
//            EnterGeneralMenuMode();
        }

        private void UpdateInput_TitleScreenMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Enter))
            {
                LoadGame();

                if (progress >= 2)
                {
                    party.Join(archer);
                }
                if (progress >= 4)
                {
                    party.Join(knight);
                }
                if (progress >= 6)
                {
                    party.Join(mage);
                }

                if (progress != 0)
                {
                    textMessage[0] = "Loading saved game...";
                    textMessageLength = 1;
                }
                else
                {
                    textMessage[0] = "No saved game found. Starting";
                    textMessage[1] = "new game...";
                    textMessageLength = 2;
                    SetUpInitialParty();        // 2015.11.14 Set up the characters
                }
                returnGameMode = GameModes.AfterTitleScreenMode;
                waitForKeyPress = true;
                moveOut = true;
                EnterDisplayTextMessageMode();
            }
        }

        private void UpdateInput_StoryMode()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.A) || newState.IsKeyDown(Keys.S) || newState.IsKeyDown(Keys.D))
                UpdateState_StoryMode();
        }

        private void UpdateState_StoryMode()
        {
            textMessageLength = LoadNextStoryLine(story);
            if (textMessageLength > 0)
            {
                returnGameMode = GameModes.StoryMode;
                waitForKeyPress = true;
                moveOut = false;
                EnterDisplayTextMessageMode();
            }
            else
                EnterInitializeBattle1Mode();
        }

        private void EnterPlayerMoveMode()
        {
            map.EmptyMapViable();
            map.EmptyMapMarked();
            if (party.Members[party.MemberOnTurn].Asleep > 0)
            {
                party.Members[party.MemberOnTurn].Asleep--;
                if (party.Members[party.MemberOnTurn].Asleep == 0)
                    textMessage[0] = party.Members[party.MemberOnTurn].Name + " has awakened.";
                else
                    textMessage[0] = party.Members[party.MemberOnTurn].Name + " fell asleep.";
                textMessageLength = 1;
                returnGameMode = GameModes.NextCharacterAfterSleepMode;
                waitForKeyPress = true;
                moveOut = true;
                EnterDisplayTextMessageMode();
            }
            else if (party.Members[party.MemberOnTurn].Stunned > 0)
            {
                party.Members[party.MemberOnTurn].Stunned--;
                // *** check if these messages correspond to the original game
                if (party.Members[party.MemberOnTurn].Stunned == 0)
                    textMessage[0] = party.Members[party.MemberOnTurn].Name + " is no longer stunned.";
                else
                    textMessage[0] = party.Members[party.MemberOnTurn].Name + " is stunned.";
                textMessageLength = 1;
                returnGameMode = GameModes.NextCharacterAfterSleepMode;
                waitForKeyPress = true;
                moveOut = true;
                EnterDisplayTextMessageMode();
            }
            else
            {
                GameMode = GameModes.PlayerMoveMode;
                map.CalcViable(party.Members[party.MemberOnTurn].Position, party.Members[party.MemberOnTurn].MovePoints, party.Members[party.MemberOnTurn].Flying, enemies);
            }
        }

        private void EnterSelectionBarTransitionMode()
        {
            GameMode = GameModes.SelectionBarTransitionMode;
            selectionBar.Position = map.CalcPosition(backUpPosition);
            oldSelectionBarPosition = selectionBar.Position;
            map.CalcBestPath((int)party.Members[party.MemberOnTurn].Position, (int)backUpPosition, party.Members[party.MemberOnTurn].MovePoints);
            currentStepNumber = 0;
        }

        private void EnterPlayerMoveTransitionMode()
        {
            GameMode = GameModes.PlayerMoveTransitionMode;
            party.Members[party.MemberOnTurn].Visible = true;
            oldMapPosition = map.Position;
            oldSelectionBarPosition = selectionBar.Position;
            if (!map.IsVisibleMinusBorder(party.Members[party.MemberOnTurn].Position, oldMapPosition))
                map.Position = mapPositionPlayerMoveMode[party.MemberOnTurn];
            selectionBar.Position = map.CalcPosition(party.Members[party.MemberOnTurn].Position);
        }

        private void EnterPlayerMovingMode()
        {
            GameMode = GameModes.PlayerMovingMode;
            delayPosition.X *= Map.TILESIZEX;
            delayPosition.Y *= Map.TILESIZEY;
        }

        private void EnterPlayerMovingInSelectionBarTransitionMode()
        {
            GameMode = GameModes.PlayerMovingInSelectionBarTransitionMode;
            delayPosition = map.CalcPosition(party.Members[party.MemberOnTurn].Position);
            delayPosition.X *= Map.TILESIZEX;
            delayPosition.Y *= Map.TILESIZEY;
        }

        private void EnterBattleMenuMode()
        {
            GameMode = GameModes.BattleMenuMode;
            map.EmptyMapViable();
            bool enemyInRange = false;
            map.EmptyMapMarked();
            for (int i = party.Members[party.MemberOnTurn].MinAttackRange; i <= party.Members[party.MemberOnTurn].MaxAttackRange; i++)
            {
                map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);
                if (map.AnyCharacterLocatedInMarkedFields(enemies))
                    enemyInRange = true;
            }
            if (enemyInRange)
                battleMenu.CurrentState = BattleMenu.States.AttackSelected1;
            else
                battleMenu.CurrentState = BattleMenu.States.StaySelected1;

            // 2024.12.02 introduced constants
            battleMenu.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 180) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 120);
            captionBox.Size = new Vector2(160, 48);
            captionBox.Position = new Vector2(Game1.PREFERREDBACKBUFFERWIDTH - 30 - captionBox.Size.X, Game1.PREFERREDBACKBUFFERHEIGHT - 30 - captionBox.Size.Y);
        }

        private void EnterBattleMenuMoveInMode()
        {
            GameMode = GameModes.BattleMenuMoveInMode;
            map.EmptyMapViable();
            bool enemyInRange = false;
            map.EmptyMapMarked();
            for (int i = party.Members[party.MemberOnTurn].MinAttackRange; i <= party.Members[party.MemberOnTurn].MaxAttackRange; i++)
            {
                map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);
                if (map.AnyCharacterLocatedInMarkedFields(enemies))
                {
                    enemyInRange = true;
                    break;
                }
            }
            if (enemyInRange)
                battleMenu.CurrentState = BattleMenu.States.AttackSelected1;
            else
                battleMenu.CurrentState = BattleMenu.States.StaySelected1;

            // 2024.12.02 introduced constants
            tempMenuPosition = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 180) / 2, Game1.PREFERREDBACKBUFFERHEIGHT);
            battleMenu.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 180) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 120);
        }

        private void EnterLargeCharacterStatsMode()
        {
            GameMode = GameModes.LargeCharacterStatsMode;
        }

        private void EnterSmallCharacterStatsMode()
        {
            GameMode = GameModes.SmallCharacterStatsMode;
            party.Members[party.MemberOnTurn].Visible = true;
        }

        private void EnterAttackMenuMode()
        {
            GameMode = GameModes.AttackMenuMode;
            oldSelectionBarPosition = map.CalcPosition(party.Members[party.MemberOnTurn].Position);
            skip = 0;
            UpdateInput_AttackMenuMode_CursorKey_Common();
        }

        private void EnterSelectionBarTransitionOutAttackMenuMode()
        {
            GameMode = GameModes.SelectionBarTransitionOutAttackMenuMode;
            tempSelectionBarPosition.X = selectionBar.Position.X * Map.TILESIZEX + SelectionBar.OFFSETX;
            tempSelectionBarPosition.Y = selectionBar.Position.Y * Map.TILESIZEY + SelectionBar.OFFSETY;
            oldSelectionBarPosition = map.CalcPosition(party.Members[party.MemberOnTurn].Position);
            selectionBar.Position = oldSelectionBarPosition;
        }

        private void EnterGeneralMenuMode()
        {
            GameMode = GameModes.GeneralMenuMode;
            captionBox.Size = new Vector2(160, 48);
            // 2024.12.02 introduced constants
            captionBox.Position = new Vector2(Game1.PREFERREDBACKBUFFERWIDTH - 30 - captionBox.Size.X, Game1.PREFERREDBACKBUFFERHEIGHT - 30 - captionBox.Size.Y);
        }

        private void EnterGeneralMenuMoveInMode()
        {
            GameMode = GameModes.GeneralMenuMoveInMode;
            generalMenu.CurrentState = GeneralMenu.States.MemberSelected2;
            // 2024.12.02 introduced constants
            tempMenuPosition = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 180) / 2, Game1.PREFERREDBACKBUFFERHEIGHT);
            generalMenu.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 180) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 120);
        }

        private void EnterItemMenuMode()
        {
            GameMode = GameModes.ItemMenuMode;
            captionBox.Size = new Vector2(160, 48);
            // 2024.12.02 introduced constants
            captionBox.Position = new Vector2(Game1.PREFERREDBACKBUFFERWIDTH - 30 - captionBox.Size.X, Game1.PREFERREDBACKBUFFERHEIGHT - 30 - captionBox.Size.Y);
        }

        private void EnterItemMenuMoveOutMode()
        {
            GameMode = GameModes.ItemMenuMoveOutMode;
            // 2024.12.02 introduced constants
            itemMenu.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 180) / 2, Game1.PREFERREDBACKBUFFERHEIGHT);
            tempMenuPosition = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 180) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 120);
        }

        private void EnterItemMenuMoveInMode()
        {
            GameMode = GameModes.ItemMenuMoveInMode;
            itemMenu.CurrentState = ItemMenu.States.UseSelected2;
            // 2024.12.02 introduced constants
            tempMenuPosition = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 180) / 2, Game1.PREFERREDBACKBUFFERHEIGHT);
            itemMenu.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 180) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 120);
            map.EmptyMapMarked();
        }

        private void EnterItemMagicSelectionMenuMode()
        {
            GameMode = GameModes.ItemMagicSelectionMenuMode;
            if (battleMenu.CurrentState == BattleMenu.States.ItemSelected1 || battleMenu.CurrentState == BattleMenu.States.ItemSelected2)
            {
                captionBox.Size = new Vector2(200, 80);
                // 2024.12.02 introduced constants
                captionBox.Position = new Vector2(Game1.PREFERREDBACKBUFFERWIDTH - 30 - captionBox.Size.X, Game1.PREFERREDBACKBUFFERHEIGHT - 30 - captionBox.Size.Y);
                map.EmptyMapMarked();
                itemMenu.CurrentState = ItemMenu.States.UseSelected1;
                // In this game, items can only be used, not given, equipped or discarded.
                switch (itemMenu.CurrentState)
                {
                    case ItemMenu.States.UseSelected1:
                    case ItemMenu.States.UseSelected2:
                        if (party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell != null)
                        {
                            map.EmptyMapMarked();
                            for (int i = party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]; i <= party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]; i++)
                                map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);
                        }
                        else
                            map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, 0);
                        break;

                    case ItemMenu.States.GiveSelected1:
                    case ItemMenu.States.GiveSelected2:
                        map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, 1);
                        break;

                    case ItemMenu.States.EquipSelected1:
                    case ItemMenu.States.EquipSelected2:
                        map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, 0);
                        break;

                    case ItemMenu.States.DropSelected1:
                    case ItemMenu.States.DropSelected2:
                        map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, 0);
                        break;
                }
            }
            else if (battleMenu.CurrentState == BattleMenu.States.MagicSelected1 || battleMenu.CurrentState == BattleMenu.States.MagicSelected2)
            {
                captionBox.Size = new Vector2(160, 80);
                // 2024.12.02 introduced constants
                captionBox.Position = new Vector2(Game1.PREFERREDBACKBUFFERWIDTH - 30 - 200, Game1.PREFERREDBACKBUFFERHEIGHT - 30 - 80);
                map.EmptyMapMarked();
                for (int i = party.Members[party.MemberOnTurn].MagicSpells[0].MinRange[party.Members[party.MemberOnTurn].MagicSpells[0].Level - 1]; i <= party.Members[party.MemberOnTurn].MagicSpells[0].MaxRange[party.Members[party.MemberOnTurn].MagicSpells[0].Level - 1]; i++)
                    map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);
            }
        }

        private void EnterItemMagicSelectionMenuMoveInMode()
        {
            GameMode = GameModes.ItemMagicSelectionMenuMoveInMode;
            itemMagicSelectionMenu.CurrentState = ItemMagicSelectionMenu.States.TopSelected2;
            // 2024.12.02 introduced constants
            tempMenuPosition = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 120) / 2, Game1.PREFERREDBACKBUFFERHEIGHT);
            itemMagicSelectionMenu.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 120) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 120);
            selectedItemNumber = 0;
            selectedSpellNumber = 0;
        }

        private void EnterItemMagicSelectionMenuMoveOutMode()
        {
            GameMode = GameModes.ItemMagicSelectionMenuMoveOutMode;
            // 2024.12.02 introduced constants
            tempMenuPosition = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 120) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 120);
            itemMagicSelectionMenu.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 120) / 2, Game1.PREFERREDBACKBUFFERHEIGHT);
        }

        private void EnterDisplayTextMessageMode()
        {
            positionInTextMessage.Row = 0;
            positionInTextMessage.Column = 0;
            textMessageDelay = 0;
            textMessageCounter = 0;
            // 2024.12.02 introduced constants
            textMessageBox.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - textMessageBox.Size.X) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 30 - textMessageBox.Size.Y);
            arrowforward.Position = textMessageBox.Position + textMessageBox.Size - new Vector2(34, 30);
            //keyRelieved = false;
            textMessageContinueFlag = false;
            GameMode = GameModes.DisplayTextMessageMode;
        }

        private void EnterDisplayTextMessageMoveOutMode()
        {
            tempTextMessageBoxPosition = textMessageBox.Position;
            textMessageBox.Position = new Vector2(textMessageBox.Position.X, textMessageBox.Position.Y + 200);
            GameMode = GameModes.DisplayTextMessageMoveOutMode;
        }

        private void EnterDropItemMode()
        {
            returnGameMode = GameModes.DropItemMode;
            yesNoButtons.CurrentState = YesNoButtons.States.YesSelected1;
            captionBox.Position = yesNoButtons.Position + new Vector2(2 * 60 + 40 + 20, 0);
            captionBox.Size = new Vector2(100, 48);
            textMessage[0] = "The " + GetSelectedItem().Name1 + GetSelectedItem().Name2 + " will be";
            textMessage[1] = "discarded.  Are you sure?";
            textMessageLength = 2;
            waitForKeyPress = false;
            moveOut = false;
            EnterDisplayTextMessageMode();
        }

        private void EnterGiveItemMode()
        {
            GameMode = GameModes.GiveItemMode;
            skip = 0;
            UpdateInput_GiveItemMode_CursorKey_Common();
        }

        private void EnterSelectionBarTransitionOutGiveItemMode()
        {
            GameMode = GameModes.SelectionBarTransitionOutGiveItemMode;
            tempSelectionBarPosition.X = selectionBar.Position.X * Map.TILESIZEX + SelectionBar.OFFSETX;
            tempSelectionBarPosition.Y = selectionBar.Position.Y * Map.TILESIZEY + SelectionBar.OFFSETY;
            oldSelectionBarPosition = map.CalcPosition(party.Members[party.MemberOnTurn].Position);
            selectionBar.Position = oldSelectionBarPosition;
        }

        private void EnterSwapItemMode()
        {
            GameMode = GameModes.SwapItemMode;
            captionBox.Size = new Vector2(200, 80);
            // 2024.12.02 introduced constants
            captionBox.Position = new Vector2(Game1.PREFERREDBACKBUFFERWIDTH - 30 - captionBox.Size.X, Game1.PREFERREDBACKBUFFERHEIGHT - 30 - captionBox.Size.Y);
        }

        private void EnterSwapItemMoveInMode()
        {
            GameMode = GameModes.SwapItemMoveInMode;
            swapMenu.CurrentState = ItemMagicSelectionMenu.States.TopSelected2;
            selectedSwapItemNumber = 0;
            // 2024.12.02 introduced constants
            tempMenuPosition = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 120) / 2, Game1.PREFERREDBACKBUFFERHEIGHT);
            swapMenu.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 120) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 120);
        }

        private void EnterSwapItemMoveOutMode()
        {
            GameMode = GameModes.SwapItemMoveOutMode;
            // 2024.12.02 introduced constants
            tempMenuPosition = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 120) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 120);
            swapMenu.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 120) / 2, Game1.PREFERREDBACKBUFFERHEIGHT);
        }

        private void EnterEquipWeaponMode()
        {
            GameMode = GameModes.EquipWeaponMode;
            captionBox.Size = new Vector2(200, 80);
            // 2024.12.02 introduced constants
            captionBox.Position = new Vector2(Game1.PREFERREDBACKBUFFERWIDTH - 30 - captionBox.Size.X, Game1.PREFERREDBACKBUFFERHEIGHT - 30 - captionBox.Size.Y);
            equipSpecsBox.Size = new Vector2(200, 142);
            equipSpecsBox.Position = new Vector2(30, Game1.PREFERREDBACKBUFFERHEIGHT - 30 - equipSpecsBox.Size.Y);
            backUpEquippedWeapon = party.Members[party.MemberOnTurn].GetEquippedWeapon();
            party.Members[party.MemberOnTurn].Unequip(backUpEquippedWeapon);
            party.Members[party.MemberOnTurn].Equip(weapons[selectedWeaponNumber]);
            map.EmptyMapMarked();
            for (int i = party.Members[party.MemberOnTurn].MinAttackRange; i <= party.Members[party.MemberOnTurn].MaxAttackRange; i++)
                map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);
        }

        private void EnterEquipWeaponMoveInMode()
        {
            GameMode = GameModes.EquipWeaponMoveInMode;
            for (int i = 0; i < 4; i++)
                weapons[i] = party.Members[party.MemberOnTurn].GetNextEquippableWeapon(i);
            if (party.Members[party.MemberOnTurn].GetEquippedWeapon() != -1)
            {
                if (party.Members[party.MemberOnTurn].GetEquippedWeapon() == weapons[0])
                {
                    equipWeaponMenu.CurrentState = ItemMagicSelectionMenu.States.TopSelected2;
                    selectedWeaponNumber = 0;
                }
                else if (party.Members[party.MemberOnTurn].GetEquippedWeapon() == weapons[1])
                {
                    equipWeaponMenu.CurrentState = ItemMagicSelectionMenu.States.LeftSelected2;
                    selectedWeaponNumber = 1;
                }
                else if (party.Members[party.MemberOnTurn].GetEquippedWeapon() == weapons[2])
                {
                    equipWeaponMenu.CurrentState = ItemMagicSelectionMenu.States.RightSelected2;
                    selectedWeaponNumber = 2;
                }
                else if (party.Members[party.MemberOnTurn].GetEquippedWeapon() == weapons[3])
                {
                    equipWeaponMenu.CurrentState = ItemMagicSelectionMenu.States.BottomSelected2;
                    selectedWeaponNumber = 3;
                }
            }
            else
            {
                equipWeaponMenu.CurrentState = ItemMagicSelectionMenu.States.BottomSelected2;
                selectedWeaponNumber = 3;
            }
            // 2024.12.02 introduced constants
            tempMenuPosition = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 120) / 2, Game1.PREFERREDBACKBUFFERHEIGHT);
            equipWeaponMenu.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 120) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 120);
        }

        private void EnterEquipWeaponMoveOutMode()
        {
            GameMode = GameModes.EquipWeaponMoveOutMode;
            // 2024.12.02 introduced constants
            tempMenuPosition = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 120) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 120);
            equipWeaponMenu.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 120) / 2, Game1.PREFERREDBACKBUFFERHEIGHT);
        }

        private void EnterEquipRingMode()
        {
            GameMode = GameModes.EquipRingMode;
            captionBox.Size = new Vector2(200, 80);
            // 2024.12.02 introduced constants
            captionBox.Position = new Vector2(Game1.PREFERREDBACKBUFFERWIDTH - 30 - captionBox.Size.X, Game1.PREFERREDBACKBUFFERHEIGHT - 30 - captionBox.Size.Y);
            equipSpecsBox.Size = new Vector2(200, 142);
            equipSpecsBox.Position = new Vector2(30, Game1.PREFERREDBACKBUFFERHEIGHT - 30 - equipSpecsBox.Size.Y);
            backUpEquippedRing = party.Members[party.MemberOnTurn].GetEquippedRing();
            party.Members[party.MemberOnTurn].Unequip(backUpEquippedRing);
            party.Members[party.MemberOnTurn].Equip(rings[selectedRingNumber]);
            map.EmptyMapMarked();
            for (int i = party.Members[party.MemberOnTurn].MinAttackRange; i <= party.Members[party.MemberOnTurn].MaxAttackRange; i++)
                map.MarkFieldsWithDistance(party.Members[party.MemberOnTurn].Position, i);
        }

        private void EnterEquipRingMoveInMode()
        {
            GameMode = GameModes.EquipRingMoveInMode;
            for (int i = 0; i < 4; i++)
                rings[i] = party.Members[party.MemberOnTurn].GetNextEquippableRing(i);
            if (party.Members[party.MemberOnTurn].GetEquippedRing() != -1)
            {
                if (party.Members[party.MemberOnTurn].GetEquippedRing() == rings[0])
                {
                    equipRingMenu.CurrentState = ItemMagicSelectionMenu.States.TopSelected2;
                    selectedRingNumber = 0;
                }
                else if (party.Members[party.MemberOnTurn].GetEquippedRing() == rings[1])
                {
                    equipRingMenu.CurrentState = ItemMagicSelectionMenu.States.LeftSelected2;
                    selectedRingNumber = 1;
                }
                else if (party.Members[party.MemberOnTurn].GetEquippedRing() == rings[2])
                {
                    equipRingMenu.CurrentState = ItemMagicSelectionMenu.States.RightSelected2;
                    selectedRingNumber = 2;
                }
                else if (party.Members[party.MemberOnTurn].GetEquippedRing() == rings[3])
                {
                    equipRingMenu.CurrentState = ItemMagicSelectionMenu.States.BottomSelected2;
                    selectedRingNumber = 3;
                }
            }
            else
            {
                equipRingMenu.CurrentState = ItemMagicSelectionMenu.States.BottomSelected2;
                selectedRingNumber = 3;
            }
            // 2024.12.02 introduced constants
            tempMenuPosition = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 120) / 2, Game1.PREFERREDBACKBUFFERHEIGHT);
            equipRingMenu.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 120) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 120);
        }

        private void EnterEquipRingMoveOutMode()
        {
            GameMode = GameModes.EquipRingMoveOutMode;
            // 2024.12.02 introduced constants
            tempMenuPosition = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 120) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 120);
            equipRingMenu.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 120) / 2, Game1.PREFERREDBACKBUFFERHEIGHT);
        }

        private void EnterPlayerBattleMode()
        {
            GameMode = GameModes.PlayerBattleMode;
            battleModeState = 0;
            currentTargetAttackedOrHealed = 0;
            numTargetsAttackedOrHealed = 1;   // will be incremented

            switch (battleMenu.CurrentState)
            {
                case BattleMenu.States.AttackSelected1:
                case BattleMenu.States.AttackSelected2:
                    if (party.Members[party.MemberOnTurn].Confused > 0)
                        targetsAttackedOrHealed[0] = target;
                    else
                    {
                        targetsAttackedOrHealed[0].WhichOne = map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip);
                        targetsAttackedOrHealed[0].BelongsToSide = CharacterPointer.Sides.CPU_Opponents;
                    }
                    break;

                case BattleMenu.States.ItemSelected1:
                case BattleMenu.States.ItemSelected2:
                    selectedMagicSpell = party.Members[party.MemberOnTurn].Items[selectedItemNumber].MagicSpell;
                    selectedMagicLevel = selectedMagicSpell.Level;
                    break;

                case BattleMenu.States.MagicSelected1:
                case BattleMenu.States.MagicSelected2:
                    selectedMagicSpell = party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber];
                    break;
            }

            switch (battleMenu.CurrentState)
            {
                case BattleMenu.States.ItemSelected1:
                case BattleMenu.States.ItemSelected2:
                case BattleMenu.States.MagicSelected1:
                case BattleMenu.States.MagicSelected2:
                    switch (selectedMagicSpell.Type)
                    {
                        case Spell.Types.Attack:
                            targetsAttackedOrHealed[0].WhichOne = map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip);
                            targetsAttackedOrHealed[0].BelongsToSide = CharacterPointer.Sides.CPU_Opponents;
                            MagicAttack(map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip), enemies);
                            break;

                        case Spell.Types.Heal:
                            // **** must be tested
                            targetsAttackedOrHealed[0].WhichOne = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                            targetsAttackedOrHealed[0].BelongsToSide = CharacterPointer.Sides.CPU_Opponents;
                            MagicHeal(map.GetNextCharacterPositionLocatedInMarkedFields(party, skip), party);
                            break;
                    }
                    break;
            }

            map.EmptyMapViable();
            map.EmptyMapMarked();
        }

        private void EnterDisplayDefeatedCharactersMode()
        {
            GameMode = GameModes.DisplayDefeatedCharactersMode;
            displayDefeatedCharactersModeState = 0;
            if (characterPointer.BelongsToSide == CharacterPointer.Sides.Player)
                party.Members[party.MemberOnTurn].Visible = true;
            else if (characterPointer.BelongsToSide == CharacterPointer.Sides.CPU_Opponents)
                enemies.Members[enemies.MemberOnTurn].Visible = true;
        }

        private void EnterPlayerBattleDisplayTextMessageMode()
        {
            positionInTextMessage.Row = 0;
            positionInTextMessage.Column = 0;
            textMessageDelay = 0;
            textMessageCounter = 0;
            // 2024.12.02 introduced constants
            textMessageBox.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - textMessageBox.Size.X) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 30 - textMessageBox.Size.Y);
            arrowforward.Position = textMessageBox.Position + textMessageBox.Size - new Vector2(34, 30);
            //keyRelieved = false;
            textMessageContinueFlag = false;
            GameMode = GameModes.PlayerBattleDisplayTextMessageMode;
        }

        private void EnterBattleDisplayTextMessageMoveOutMode()
        {
            tempTextMessageBoxPosition = textMessageBox.Position;
            textMessageBox.Position = new Vector2(textMessageBox.Position.X, textMessageBox.Position.Y + 200);
            GameMode = GameModes.BattleDisplayTextMessageMoveOutMode;
        }

        private void EnterMagicLevelSelectionMode()
        {
            GameMode = GameModes.MagicLevelSelectionMode;
            selectedMagicLevel = party.Members[party.MemberOnTurn].MagicSpells[selectedSpellNumber].Level;
            Spell.CurrentState = 1;
            // 2024.12.02 introduced constants
            redbar.Position = new Vector2(Game1.PREFERREDBACKBUFFERWIDTH - 30 - 200, Game1.PREFERREDBACKBUFFERHEIGHT - 30 - 80) + new Vector2(13, 24);
        }

        private void EnterEnemyMoveTransitionMode()
        {
            GameMode = GameModes.EnemyMoveTransitionMode;
            enemies.Members[enemies.MemberOnTurn].Visible = true;
            oldMapPosition = map.Position;
            oldSelectionBarPosition = selectionBar.Position;
            if (!map.IsVisibleMinusBorder(enemies.Members[enemies.MemberOnTurn].Position, oldMapPosition))
                map.Position = mapPositionEnemyMoveMode[enemies.MemberOnTurn];
            selectionBar.Position = map.CalcPosition(enemies.Members[enemies.MemberOnTurn].Position);
        }

        private void EnterEnemyMoveMode()
        {
            map.EmptyMapViable();
            map.EmptyMapMarked();
            if (enemies.Members[enemies.MemberOnTurn].Asleep > 0)
            {
                enemies.Members[enemies.MemberOnTurn].Asleep--;
                if (enemies.Members[enemies.MemberOnTurn].Asleep == 0)
                    textMessage[0] = enemies.Members[enemies.MemberOnTurn].Name + " has awakened.";
                else
                    textMessage[0] = enemies.Members[enemies.MemberOnTurn].Name + " fell asleep.";
                textMessageLength = 1;
                returnGameMode = GameModes.NextCharacterAfterSleepMode;
                waitForKeyPress = true;
                moveOut = true;
                EnterDisplayTextMessageMode();
            }
            else if (enemies.Members[enemies.MemberOnTurn].Stunned > 0)
            {
                enemies.Members[enemies.MemberOnTurn].Stunned--;
                // *** check if these messages correspond to the original game
                if (enemies.Members[enemies.MemberOnTurn].Stunned == 0)
                    textMessage[0] = enemies.Members[enemies.MemberOnTurn].Name + " is no longer stunned.";
                else
                    textMessage[0] = enemies.Members[enemies.MemberOnTurn].Name + " is stunned.";
                textMessageLength = 1;
                returnGameMode = GameModes.NextCharacterAfterSleepMode;
                waitForKeyPress = true;
                moveOut = true;
                EnterDisplayTextMessageMode();
            }
            else
            {
                GameMode = GameModes.EnemyMoveMode;
                map.CalcViable(enemies.Members[enemies.MemberOnTurn].Position, enemies.Members[enemies.MemberOnTurn].MovePoints, enemies.Members[enemies.MemberOnTurn].Flying, party);
                enemyBattleMove = BattleMoves.Stay;
                if (enemies.Members[enemies.MemberOnTurn].Confused > 0)
                {
                    if (enemies.Members[enemies.MemberOnTurn].CanCastAttackSpell())
                        target = EnterEnemyMoveMode_MagicAttackConfused();
                    if (enemyBattleMove == BattleMoves.Stay)
                    {
                        if (enemies.Members[enemies.MemberOnTurn].HasAttackItem())
                            target = EnterEnemyMoveMode_ItemMagicAttackConfused();
                    }
                    if (enemyBattleMove == BattleMoves.Stay)
                        target = EnterEnemyMoveMode_AttackConfused();
                    if (enemyBattleMove == BattleMoves.Stay)
                        EnterEnemyMoveMode_StayConfused();
                }
                else
                {
                    target = EnterEnemyMoveMode_DefeatOpponentPartyWithOneBlow();
                    if (enemyBattleMove == BattleMoves.Stay)
                    {
                        if (enemies.Members[enemies.MemberOnTurn].CanCastHealSpell())
                            target = EnterEnemyMoveMode_MagicHeal();
                    }
                    if (enemyBattleMove == BattleMoves.Stay)
                    {
                        if (enemies.Members[enemies.MemberOnTurn].CanCastAttackSpell())
                            target = EnterEnemyMoveMode_MagicAttack();
                    }
                    if (enemyBattleMove == BattleMoves.Stay)
                    {
                        if (enemies.Members[enemies.MemberOnTurn].HasHealItem())
                            target = EnterEnemyMoveMode_ItemMagicHeal();
                    }
                    if (enemyBattleMove == BattleMoves.Stay)
                    {
                        if (enemies.Members[enemies.MemberOnTurn].HasAttackItem())
                            target = EnterEnemyMoveMode_ItemMagicAttack();
                    }
                    if (enemyBattleMove == BattleMoves.Stay)
                        target = EnterEnemyMoveMode_Attack();
                    if (enemyBattleMove == BattleMoves.Stay)
                        EnterEnemyMoveMode_Stay();
                }
                map.CalcBestPath((int)enemies.Members[enemies.MemberOnTurn].Position, (int)backUpPosition, enemies.Members[enemies.MemberOnTurn].MovePoints);
                currentStepNumber = 0;
            }
            map.EmptyMapMarked();
            map.EmptyMapViable();
            map.CalcViable(enemies.Members[enemies.MemberOnTurn].Position, enemies.Members[enemies.MemberOnTurn].MovePoints, enemies.Members[enemies.MemberOnTurn].Flying, party);
        }

        private CharacterPointer EnterEnemyMoveMode_Attack()
        {
            if (map.MarkAttackableOpponents(enemies, party, enemies.Members[enemies.MemberOnTurn].MinAttackRange, enemies.Members[enemies.MemberOnTurn].MaxAttackRange))
            {
                bool charFound = false;
                int charNumber = -1;
                // 1. check whether there is a hero that can be defeated with one blow
                skip = 0;
                while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                {
                    charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                    if (party.Members[charNumber].MustSurvive
                        && party.Members[charNumber].HitPoints <= enemies.Members[enemies.MemberOnTurn].AttackPoints - party.Members[charNumber].DefensePoints)
                    {
                        charFound = true;
                        targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                    }
                    skip++;
                }
                if (!charFound)
                {
                    // 2. check whether there is a healer with enough MP or somebody who has a heal item that can be defeated with one blow
                    skip = 0;
                    while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                    {
                        charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                        if (party.Members[charNumber].CanUseHealMagic()
                            && party.Members[charNumber].HitPoints <= enemies.Members[enemies.MemberOnTurn].AttackPoints - party.Members[charNumber].DefensePoints)
                        {
                            charFound = true;
                            targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                        }
                        skip++;
                    }
                    if (!charFound)
                    {
                        // 3. check whether there is a healer with enough MP or somebody who has a heal item
                        skip = 0;
                        while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                        {
                            charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                            if (party.Members[charNumber].CanUseHealMagic())
                            {
                                charFound = true;
                                targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                            }
                            skip++;
                        }
                        if (!charFound)
                        {
                            // 4. check whether there is a hero
                            skip = 0;
                            while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                            {
                                charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                                if (party.Members[charNumber].MustSurvive)
                                {
                                    charFound = true;
                                    targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                                }
                                skip++;
                            }
                            if (!charFound)
                            {
                                int blowNumber;
                                int blowNumber_save = -1;
                                int charNumber_save = -1;
                                // 5. get the weakest character (the one that can be defeated with the least number of blows)
                                skip = 0;
                                while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                                {
                                    charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                                    blowNumber = party.Members[charNumber].HitPoints / (enemies.Members[enemies.MemberOnTurn].AttackPoints - party.Members[charNumber].DefensePoints);
                                    if (blowNumber_save == -1 || blowNumber_save > blowNumber)
                                    {
                                        blowNumber_save = blowNumber;
                                        charNumber_save = charNumber;
                                        targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                                    }
                                    skip++;
                                }
                                charNumber = charNumber_save;
                            }
                        }
                    }
                }

                // here comes the code to get the position to which the enemy has to move
                // this algorithm chooses the closest location to the enemy's current location
                backUpPosition = map.CalcPositionWhereToMove(enemies, party, charNumber, enemies.Members[enemies.MemberOnTurn].MinAttackRange, enemies.Members[enemies.MemberOnTurn].MaxAttackRange);
                enemyBattleMove = BattleMoves.Attack;
                return new CharacterPointer(CharacterPointer.Sides.Player, charNumber);
            }
            return null;
        }

        private CharacterPointer EnterEnemyMoveMode_MagicAttack()
        {
            // *** this algorithm doesn't support muddle, dispel, desoul, slow yet

            selectedSpellNumber = Character.MAXNUMBER_SPELLS - 1;

            while (selectedSpellNumber >= 0)
            {
                if (enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber] != null
                    && enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].Type == Spell.Types.Attack)
                {
                    bool charFound = false;
                    int charNumber = -1;

                    // calculate strongest possible magic level
                    selectedMagicLevel = enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].Level;
                    bool found = false;
                    while (!found && selectedMagicLevel >= 1)
                        if (enemies.Members[enemies.MemberOnTurn].MagicPoints < enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MagicPoints[selectedMagicLevel - 1])
                            selectedMagicLevel--;
                        else
                            found = true;
                    if (found)
                    {
                        // choose enemy to attack
                        map.EmptyMapMarked();
                        if (map.MarkAttackableOpponents(enemies, party, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]))
                        {
                            // 1. check whether there is a hero that can be defeated with one spellcast
                            skip = 0;
                            while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                            {
                                charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                                if (party.Members[charNumber].MustSurvive
                                    && party.Members[charNumber].HitPoints <= enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].EffectPoints[selectedMagicLevel - 1])
                                {
                                    charFound = true;
                                    targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                                }
                                skip++;
                            }
                            if (!charFound)
                            {
                                // 2. check whether there is a healer with enough MP or somebody who has a heal item that can be defeated with one spellcast
                                skip = 0;
                                while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                                {
                                    charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                                    if (party.Members[charNumber].CanUseHealMagic()
                                        && party.Members[charNumber].HitPoints <= enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].EffectPoints[selectedMagicLevel - 1])
                                    {
                                        charFound = true;
                                        targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                                    }
                                    skip++;
                                }
                                if (!charFound)
                                {
                                    // 3. check whether there is a healer with enough MP or somebody who has a heal item
                                    skip = 0;
                                    while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                                    {
                                        charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                                        if (party.Members[charNumber].CanUseHealMagic())
                                        {
                                            charFound = true;
                                            targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                                        }
                                        skip++;
                                    }
                                    if (!charFound)
                                    {
                                        // 4. check whether there is a hero
                                        skip = 0;
                                        while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                                        {
                                            charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                                            if (party.Members[charNumber].MustSurvive)
                                            {
                                                charFound = true;
                                                targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                                            }
                                            skip++;
                                        }
                                        if (!charFound)
                                        {
                                            int defense_save = -1;
                                            int charNumber_save = -1;
                                            // 5. get the character with the highest defense
                                            skip = 0;
                                            while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                                            {
                                                charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                                                if (defense_save == -1 || defense_save < party.Members[charNumber].DefensePoints)
                                                {
                                                    defense_save = party.Members[charNumber].DefensePoints;
                                                    charNumber_save = charNumber;
                                                    targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                                                }
                                                skip++;
                                            }
                                            charNumber = charNumber_save;
                                        }
                                    }
                                }
                            }

                            // calculate optimum magic level
                            int effect_save = party.Members[charNumber].HitPoints - enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].EffectPoints[selectedMagicLevel - 1];
                            if (effect_save < 0)
                                effect_save = 0;
                            found = false;
                            while (!found && selectedMagicLevel >= 1)
                            {
                                int effect = party.Members[charNumber].HitPoints - enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].EffectPoints[selectedMagicLevel - 1];
                                if (effect < 0)
                                    effect = 0;
                                map.EmptyMapMarked();
                                map.MarkAttackableOpponents(enemies, party, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]);
                                if (enemies.Members[enemies.MemberOnTurn].MagicPoints >= enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MagicPoints[selectedMagicLevel - 1]
                                    && map.IsMarked(party.Members[charNumber].Position)
                                    && effect <= effect_save)
                                {
                                    effect_save = effect;
                                    selectedMagicLevel--;
                                }
                                else
                                {
                                    selectedMagicLevel++;
                                    found = true;
                                }
                            }
                            if (!found)
                                selectedMagicLevel = 1;

                            // is a regular attack possible?
                            map.EmptyMapMarked();
                            for (int i = enemies.Members[enemies.MemberOnTurn].MinAttackRange; i <= enemies.Members[enemies.MemberOnTurn].MaxAttackRange; i++)
                                map.MarkFieldsWithDistance(enemies.Members[enemies.MemberOnTurn].Position, i);
                            skip = 0;
                            bool regularAttackPossible = false;
                            while (!regularAttackPossible && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                                if (map.GetNextCharacterNumberLocatedInMarkedFields(party, skip) == charNumber)
                                    regularAttackPossible = true;
                                else
                                    skip++;

                            map.EmptyMapMarked();
                            map.MarkAttackableOpponents(enemies, party, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]);

                            // if a regular attack is possible and at least equally effective as a spellcast, then don't cast the spell
                            int effectMagic = party.Members[charNumber].HitPoints - enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].EffectPoints[selectedMagicLevel - 1];
                            if (effectMagic < 0)
                                effectMagic = 0;
                            int effectAttack = party.Members[charNumber].HitPoints + party.Members[charNumber].DefensePoints - enemies.Members[enemies.MemberOnTurn].AttackPoints;
                            if (effectAttack < 0)
                                effectAttack = 0;

                            // here was a bug in the original remake!!
                            if (!regularAttackPossible || effectMagic > effectAttack)
                            {
                                // if the area is >= 2, check what is the position with which the biggest number of opponents is affected
                                if (enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].Area[selectedMagicLevel - 1] >= 2)
                                {
                                    map.EmptyMapMarked();
                                    map.MarkAttackableOpponents(enemies, party, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]);
                                    numTargetsAttackedOrHealed = 1;
                                    selectedMagicSpell = enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber];
                                    MagicAttack(targetPosition, party);
                                    int numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                    int targetPosition_save = targetPosition;
                                    if (map.IsMarked(targetPosition - (int)map.Size.X) && map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition - (int)map.Size.X) != -1)
                                    {
                                        numTargetsAttackedOrHealed = 1;
                                        MagicAttack(targetPosition - (int)map.Size.X, party);
                                        if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                        {
                                            targetPosition_save = targetPosition - (int)map.Size.X;
                                            numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                        }
                                    }
                                    if (map.IsMarked(targetPosition - 1) && map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition - 1) != -1)
                                    {
                                        numTargetsAttackedOrHealed = 1;
                                        MagicAttack(targetPosition - 1, party);
                                        if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                        {
                                            targetPosition_save = targetPosition - 1;
                                            numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                        }
                                    }
                                    if (map.IsMarked(targetPosition + 1) && map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition + 1) != -1)
                                    {
                                        numTargetsAttackedOrHealed = 1;
                                        MagicAttack(targetPosition + 1, party);
                                        if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                        {
                                            targetPosition_save = targetPosition + 1;
                                            numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                        }
                                    }
                                    if (map.IsMarked(targetPosition + (int)map.Size.X) && map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition + (int)map.Size.X) != -1)
                                    {
                                        numTargetsAttackedOrHealed = 1;
                                        MagicAttack(targetPosition + (int)map.Size.X, party);
                                        if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                        {
                                            targetPosition_save = targetPosition + (int)map.Size.X;
                                            numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                        }
                                    }
                                    targetPosition = targetPosition_save;
                                    charNumber = map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition);
                                }

                                // here comes the code to get the position to which the enemy has to move
                                // this algorithm chooses the closest location to the enemy's current location
                                backUpPosition = map.CalcPositionWhereToMove(enemies, party, charNumber, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]);
                                enemyBattleMove = BattleMoves.MagicAttack;
                                return new CharacterPointer(CharacterPointer.Sides.Player, charNumber);
                            }
                        }
                    }
                }
                selectedSpellNumber--;
            }

            return null;
        }

        private CharacterPointer EnterEnemyMoveMode_ItemMagicAttack()
        {
            // *** this algorithm doesn't support muddle, dispel, desoul, slow yet

            bool charFound = false;
            int charNumber = -1;

            selectedItemNumber = 0;

            while (selectedItemNumber < Character.MAXNUMBER_ITEMS)
            {
                if (enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber] != null
                    && enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell != null
                    && enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Type == Spell.Types.Attack)
                {
                    // choose enemy to attack
                    map.EmptyMapMarked();
                    if (map.MarkAttackableOpponents(enemies, party, enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1], enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]))
                    {
                        // 1. check whether there is a hero that can be defeated with one spellcast
                        skip = 0;
                        while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                        {
                            charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                            if (party.Members[charNumber].MustSurvive
                                && party.Members[charNumber].HitPoints <= enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.EffectPoints[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1])
                            {
                                charFound = true;
                                targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                            }
                            skip++;
                        }
                        if (!charFound)
                        {
                            // 2. check whether there is a healer with enough MP or somebody who has a heal item that can be defeated with one spellcast
                            skip = 0;
                            while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                            {
                                charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                                if (party.Members[charNumber].CanUseHealMagic()
                                    && party.Members[charNumber].HitPoints <= enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.EffectPoints[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1])
                                {
                                    charFound = true;
                                    targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                                }
                                skip++;
                            }
                            if (!charFound)
                            {
                                // 3. check whether there is a healer with enough MP or somebody who has a heal item
                                skip = 0;
                                while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                                {
                                    charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                                    if (party.Members[charNumber].CanUseHealMagic())
                                    {
                                        charFound = true;
                                        targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                                    }
                                    skip++;
                                }
                                if (!charFound)
                                {
                                    // 4. check whether there is a hero
                                    skip = 0;
                                    while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                                    {
                                        charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                                        if (party.Members[charNumber].MustSurvive)
                                        {
                                            charFound = true;
                                            targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                                        }
                                        skip++;
                                    }
                                    if (!charFound)
                                    {
                                        int defense_save = -1;
                                        int charNumber_save = -1;
                                        // 5. get the character with the highest defense
                                        skip = 0;
                                        while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                                        {
                                            charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                                            if (defense_save == -1 || defense_save < party.Members[charNumber].DefensePoints)
                                            {
                                                defense_save = party.Members[charNumber].DefensePoints;
                                                charNumber_save = charNumber;
                                                targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                                            }
                                            skip++;
                                        }
                                        charNumber = charNumber_save;
                                    }
                                }
                            }
                        }

                        // is a regular attack possible?
                        map.EmptyMapMarked();
                        for (int i = enemies.Members[enemies.MemberOnTurn].MinAttackRange; i <= enemies.Members[enemies.MemberOnTurn].MaxAttackRange; i++)
                            map.MarkFieldsWithDistance(enemies.Members[enemies.MemberOnTurn].Position, i);
                        skip = 0;
                        bool regularAttackPossible = false;
                        while (!regularAttackPossible && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                            if (map.GetNextCharacterNumberLocatedInMarkedFields(party, skip) == charNumber)
                                regularAttackPossible = true;
                            else
                                skip++;

                        map.EmptyMapMarked();
                        map.MarkAttackableOpponents(enemies, party, enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1], enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]);

                        // if a regular attack is possible and at least equally effective as an item spellcast, then don't use the item
                        int effectMagic = party.Members[charNumber].HitPoints - enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.EffectPoints[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1];
                        if (effectMagic < 0)
                            effectMagic = 0;
                        int effectAttack = party.Members[charNumber].HitPoints + party.Members[charNumber].DefensePoints - enemies.Members[enemies.MemberOnTurn].AttackPoints;
                        if (effectAttack < 0)
                            effectAttack = 0;

                        if (!regularAttackPossible || effectMagic < effectAttack)
                        {
                            selectedMagicSpell = enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell;
                            selectedMagicLevel = selectedMagicSpell.Level;
                            // if the area is >= 2, check what is the position with which the biggest number of opponents is affected
                            if (selectedMagicSpell.Area[selectedMagicLevel - 1] >= 2)
                            {
                                map.EmptyMapMarked();
                                map.MarkAttackableOpponents(enemies, party, enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1], enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]);
                                numTargetsAttackedOrHealed = 1;
                                MagicAttack(targetPosition, party);
                                int numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                int targetPosition_save = targetPosition;
                                if (map.IsMarked(targetPosition - (int)map.Size.X) && map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition - (int)map.Size.X) != -1)
                                {
                                    numTargetsAttackedOrHealed = 1;
                                    MagicAttack(targetPosition - (int)map.Size.X, party);
                                    if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                    {
                                        targetPosition_save = targetPosition - (int)map.Size.X;
                                        numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                    }
                                }
                                if (map.IsMarked(targetPosition - 1) && map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition - 1) != -1)
                                {
                                    numTargetsAttackedOrHealed = 1;
                                    MagicAttack(targetPosition - 1, party);
                                    if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                    {
                                        targetPosition_save = targetPosition - 1;
                                        numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                    }
                                }
                                if (map.IsMarked(targetPosition + 1) && map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition + 1) != -1)
                                {
                                    numTargetsAttackedOrHealed = 1;
                                    MagicAttack(targetPosition + 1, party);
                                    if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                    {
                                        targetPosition_save = targetPosition + 1;
                                        numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                    }
                                }
                                if (map.IsMarked(targetPosition + (int)map.Size.X) && map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition + (int)map.Size.X) != -1)
                                {
                                    numTargetsAttackedOrHealed = 1;
                                    MagicAttack(targetPosition + (int)map.Size.X, party);
                                    if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                    {
                                        targetPosition_save = targetPosition + (int)map.Size.X;
                                        numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                    }
                                }
                                targetPosition = targetPosition_save;
                                charNumber = map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition);
                            }

                            // here comes the code to get the position to which the enemy has to move
                            // this algorithm chooses the closest location to the enemy's current location
                            backUpPosition = map.CalcPositionWhereToMove(enemies, party, charNumber, enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1], enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]);
                            enemyBattleMove = BattleMoves.ItemMagicAttack;
                            return new CharacterPointer(CharacterPointer.Sides.Player, charNumber);
                        }
                    }
                }
                selectedItemNumber++;
            }

            return null;
        }

        private CharacterPointer EnterEnemyMoveMode_MagicHeal()
        {
            // *** detox still has to be implemented
            // *** if HEAL is level 4, this algorithm will only search for healable characters in the range of level 4, but not in the larger range of level 3

            bool charFound = false;
            int charNumber = -1;

            selectedSpellNumber = Character.MAXNUMBER_SPELLS - 1;

            while (selectedSpellNumber >= 0)
            {
                if (enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber] != null
                    && enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].Type == Spell.Types.Heal)
                {
                    // calculate strongest possible magic level
                    selectedMagicLevel = enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].Level;
                    bool found = false;
                    while (!found && selectedMagicLevel >= 1)
                        if (enemies.Members[enemies.MemberOnTurn].MagicPoints < enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MagicPoints[selectedMagicLevel - 1])
                            selectedMagicLevel--;
                        else
                            found = true;
                    if (found)
                    {
                        // choose ally to heal
                        map.EmptyMapMarked();
                        if (map.MarkHealableAllies(enemies, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]))
                        {
                            switch (enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].Name)
                            {
                                case "BOOST":
                                    // check whether there is an unboosted ally
                                    skip = 0;
                                    bool unboostedHimself = false;
                                    while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip) != -1)
                                    {
                                        charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip);
                                        if (enemies.Members[charNumber].AgilityBoostedBy == 0)
                                        {
                                            targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip);
                                            if (enemies.MemberOnTurn == charNumber)
                                                unboostedHimself = true;
                                            else
                                                charFound = true;
                                        }
                                        skip++;
                                    }

                                    if (unboostedHimself)
                                        charFound = true;

                                    if (charFound)
                                    {
                                        // calculate optimum magic level
                                        found = false;
                                        while (!found && selectedMagicLevel >= 1)
                                        {
                                            map.EmptyMapMarked();
                                            map.MarkHealableAllies(enemies, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]);
                                            if (enemies.Members[enemies.MemberOnTurn].MagicPoints >= enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MagicPoints[selectedMagicLevel - 1]
                                                && map.IsMarked(enemies.Members[charNumber].Position))
                                                selectedMagicLevel--;
                                            else
                                            {
                                                selectedMagicLevel++;
                                                found = true;
                                            }
                                        }
                                        if (!found)
                                            selectedMagicLevel = 1;

                                        // if the area is >= 2, check what is the position with which the biggest number of allies is affected
                                        if (enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].Area[selectedMagicLevel - 1] >= 2)
                                        {
                                            map.EmptyMapMarked();
                                            map.MarkHealableAllies(enemies, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]);
                                            numTargetsAttackedOrHealed = 1;
                                            selectedMagicSpell = enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber];
                                            Boost(targetPosition, enemies);
                                            int numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                            int targetPosition_save = targetPosition;
                                            if (map.IsMarked(targetPosition - (int)map.Size.X) && map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition - (int)map.Size.X) != -1)
                                            {
                                                numTargetsAttackedOrHealed = 1;
                                                Boost(targetPosition - (int)map.Size.X, enemies);
                                                if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                                {
                                                    targetPosition_save = targetPosition - (int)map.Size.X;
                                                    numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                                }
                                            }
                                            if (map.IsMarked(targetPosition - 1) && map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition - 1) != -1)
                                            {
                                                numTargetsAttackedOrHealed = 1;
                                                Boost(targetPosition - 1, enemies);
                                                if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                                {
                                                    targetPosition_save = targetPosition - 1;
                                                    numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                                }
                                            }
                                            if (map.IsMarked(targetPosition + 1) && map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition + 1) != -1)
                                            {
                                                numTargetsAttackedOrHealed = 1;
                                                Boost(targetPosition + 1, enemies);
                                                if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                                {
                                                    targetPosition_save = targetPosition + 1;
                                                    numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                                }
                                            }
                                            if (map.IsMarked(targetPosition + (int)map.Size.X) && map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition + (int)map.Size.X) != -1)
                                            {
                                                numTargetsAttackedOrHealed = 1;
                                                Boost(targetPosition + (int)map.Size.X, enemies);
                                                if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                                {
                                                    targetPosition_save = targetPosition + (int)map.Size.X;
                                                    numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                                }
                                            }
                                            targetPosition = targetPosition_save;
                                            charNumber = map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition);
                                        }

                                        // here comes the code to get the position to which the enemy has to move
                                        // this algorithm chooses the closest location to the enemy's current location
                                        backUpPosition = map.CalcPositionWhereToMove(enemies, enemies, charNumber, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]);
                                        enemyBattleMove = BattleMoves.MagicHeal;
                                        return new CharacterPointer(CharacterPointer.Sides.CPU_Opponents, charNumber);
                                    }
                                    break;

                                default:
                                    // 1. check whether there is a "must survive" ally that has a damage of 5 hitpoints or more
                                    skip = 0;
                                    while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip) != -1)
                                    {
                                        charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip);
                                        if (enemies.Members[charNumber].MustSurvive
                                            && enemies.Members[charNumber].MaxHitPoints - enemies.Members[charNumber].HitPoints >= 5)
                                        {
                                            charFound = true;
                                            targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip);
                                        }
                                        skip++;
                                    }
                                    if (!charFound)
                                    {
                                        // 2. check whether there is ANOTHER healer with enough MP or somebody who has a heal item that has a damage of 5 hitpoints or more
                                        skip = 0;
                                        while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip) != -1)
                                        {
                                            charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip);
                                            if (enemies.Members[charNumber].CanUseHealMagic()
                                                && enemies.Members[charNumber].MaxHitPoints - enemies.Members[charNumber].HitPoints >= 5
                                                && charNumber != enemies.MemberOnTurn)
                                            {
                                                charFound = true;
                                                targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip);
                                            }
                                            skip++;
                                        }
                                        if (!charFound)
                                        {
                                            // 3. get the character with the lowest number of hitpoints
                                            int hp_save = -1;
                                            int charNumber_save = -1;
                                            skip = 0;
                                            while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip) != -1)
                                            {
                                                charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip);
                                                if (hp_save == -1 || hp_save > enemies.Members[charNumber].HitPoints)
                                                {
                                                    hp_save = enemies.Members[charNumber].HitPoints;
                                                    charNumber_save = charNumber;
                                                    targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip);
                                                }
                                                skip++;
                                            }
                                            charNumber = charNumber_save;
                                        }
                                    }

                                    // heal only if the selected character has a damage of 25% or more
                                    if (enemies.Members[charNumber].HitPoints <= enemies.Members[charNumber].MaxHitPoints * .75)
                                    {
                                        // calculate optimum magic level
                                        int effect_save = enemies.Members[charNumber].HitPoints + enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].EffectPoints[selectedMagicLevel - 1];
                                        if (effect_save > enemies.Members[charNumber].MaxHitPoints)
                                            effect_save = enemies.Members[charNumber].MaxHitPoints;
                                        found = false;
                                        while (!found && selectedMagicLevel >= 1)
                                        {
                                            int effect = enemies.Members[charNumber].HitPoints + enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].EffectPoints[selectedMagicLevel - 1];
                                            if (effect > enemies.Members[charNumber].MaxHitPoints)
                                                effect = enemies.Members[charNumber].MaxHitPoints;
                                            map.EmptyMapMarked();
                                            map.MarkHealableAllies(enemies, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]);
                                            if (enemies.Members[enemies.MemberOnTurn].MagicPoints >= enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MagicPoints[selectedMagicLevel - 1]
                                                && map.IsMarked(enemies.Members[charNumber].Position)
                                                && effect <= effect_save)
                                            {
                                                effect_save = effect;
                                                selectedMagicLevel--;
                                            }
                                            else
                                            {
                                                selectedMagicLevel++;
                                                found = true;
                                            }
                                        }
                                        if (!found)
                                            selectedMagicLevel = 1;

                                        // if the area is >= 2, check what is the position with which the biggest number of allies is affected
                                        if (enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].Area[selectedMagicLevel - 1] >= 2)
                                        {
                                            map.EmptyMapMarked();
                                            map.MarkHealableAllies(enemies, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]);
                                            numTargetsAttackedOrHealed = 1;
                                            selectedMagicSpell = enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber];
                                            MagicHeal(targetPosition, enemies);
                                            int numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                            int targetPosition_save = targetPosition;
                                            if (map.IsMarked(targetPosition - (int)map.Size.X) && map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition - (int)map.Size.X) != -1)
                                            {
                                                numTargetsAttackedOrHealed = 1;
                                                MagicHeal(targetPosition - (int)map.Size.X, enemies);
                                                if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                                {
                                                    targetPosition_save = targetPosition - (int)map.Size.X;
                                                    numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                                }
                                            }
                                            if (map.IsMarked(targetPosition - 1) && map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition - 1) != -1)
                                            {
                                                numTargetsAttackedOrHealed = 1;
                                                MagicHeal(targetPosition - 1, enemies);
                                                if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                                {
                                                    targetPosition_save = targetPosition - 1;
                                                    numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                                }
                                            }
                                            if (map.IsMarked(targetPosition + 1) && map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition + 1) != -1)
                                            {
                                                numTargetsAttackedOrHealed = 1;
                                                MagicHeal(targetPosition + 1, enemies);
                                                if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                                {
                                                    targetPosition_save = targetPosition + 1;
                                                    numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                                }
                                            }
                                            if (map.IsMarked(targetPosition + (int)map.Size.X) && map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition + (int)map.Size.X) != -1)
                                            {
                                                numTargetsAttackedOrHealed = 1;
                                                MagicHeal(targetPosition + (int)map.Size.X, enemies);
                                                if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                                {
                                                    targetPosition_save = targetPosition + (int)map.Size.X;
                                                    numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                                }
                                            }
                                            targetPosition = targetPosition_save;
                                            charNumber = map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition);
                                        }

                                        // here comes the code to get the position to which the enemy has to move
                                        // this algorithm chooses the closest location to the enemy's current location
                                        backUpPosition = map.CalcPositionWhereToMove(enemies, enemies, charNumber, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]);
                                        enemyBattleMove = BattleMoves.MagicHeal;
                                        return new CharacterPointer(CharacterPointer.Sides.CPU_Opponents, charNumber);
                                    }
                                    break;
                            }
                        }
                    }
                }
                selectedSpellNumber--;
            }

            return null;
        }

        private CharacterPointer EnterEnemyMoveMode_ItemMagicHeal()
        {
            // *** detox still has to be implemented

            bool charFound = false;
            int charNumber = -1;

            selectedItemNumber = 0;

            while (selectedItemNumber < Character.MAXNUMBER_ITEMS)
            {
                if (enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber] != null
                    && enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell != null
                    && enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Type == Spell.Types.Heal)
                {
                    // choose ally to heal
                    map.EmptyMapMarked();
                    if (map.MarkHealableAllies(enemies, enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1], enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]))
                    {
                        switch (enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Name)
                        {
                            case "BOOST":
                                // check whether there is an unboosted ally
                                skip = 0;
                                bool unboostedHimself = false;
                                while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip) != -1)
                                {
                                    charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip);
                                    if (enemies.Members[charNumber].AgilityBoostedBy == 0)
                                    {
                                        targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip);
                                        if (enemies.MemberOnTurn == charNumber)
                                            unboostedHimself = true;
                                        else
                                            charFound = true;
                                    }
                                    skip++;
                                }

                                if (unboostedHimself)
                                    charFound = true;

                                if (charFound)
                                {
                                    // if the area is >= 2, check what is the position with which the biggest number of allies is affected
                                    if (enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Area[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1] >= 2)
                                    {
                                        map.EmptyMapMarked();
                                        map.MarkHealableAllies(enemies, enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1], enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]);
                                        numTargetsAttackedOrHealed = 1;
                                        selectedMagicSpell = enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell;
                                        Boost(targetPosition, enemies);
                                        int numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                        int targetPosition_save = targetPosition;
                                        if (map.IsMarked(targetPosition - (int)map.Size.X) && map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition - (int)map.Size.X) != -1)
                                        {
                                            numTargetsAttackedOrHealed = 1;
                                            Boost(targetPosition - (int)map.Size.X, enemies);
                                            if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                            {
                                                targetPosition_save = targetPosition - (int)map.Size.X;
                                                numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                            }
                                        }
                                        if (map.IsMarked(targetPosition - 1) && map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition - 1) != -1)
                                        {
                                            numTargetsAttackedOrHealed = 1;
                                            Boost(targetPosition - 1, enemies);
                                            if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                            {
                                                targetPosition_save = targetPosition - 1;
                                                numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                            }
                                        }
                                        if (map.IsMarked(targetPosition + 1) && map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition + 1) != -1)
                                        {
                                            numTargetsAttackedOrHealed = 1;
                                            Boost(targetPosition + 1, enemies);
                                            if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                            {
                                                targetPosition_save = targetPosition + 1;
                                                numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                            }
                                        }
                                        if (map.IsMarked(targetPosition + (int)map.Size.X) && map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition + (int)map.Size.X) != -1)
                                        {
                                            numTargetsAttackedOrHealed = 1;
                                            Boost(targetPosition + (int)map.Size.X, enemies);
                                            if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                            {
                                                targetPosition_save = targetPosition + (int)map.Size.X;
                                                numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                            }
                                        }
                                        targetPosition = targetPosition_save;
                                        charNumber = map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition);
                                    }

                                    // here comes the code to get the position to which the enemy has to move
                                    // this algorithm chooses the closest location to the enemy's current location
                                    backUpPosition = map.CalcPositionWhereToMove(enemies, enemies, charNumber, enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1], enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]);
                                    enemyBattleMove = BattleMoves.MagicHeal;
                                    return new CharacterPointer(CharacterPointer.Sides.CPU_Opponents, charNumber);
                                }
                                break;

                            default:
                                // 1. check whether there is a "must survive" ally that has a damage of 5 hitpoints or more
                                skip = 0;
                                while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip) != -1)
                                {
                                    charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip);
                                    if (enemies.Members[charNumber].MustSurvive
                                        && enemies.Members[charNumber].MaxHitPoints - enemies.Members[charNumber].HitPoints >= 5)
                                    {
                                        charFound = true;
                                        targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip);
                                    }
                                    skip++;
                                }
                                if (!charFound)
                                {
                                    // 2. check whether there is ANOTHER healer with enough MP or somebody who has a heal item that has a damage of 5 hitpoints or more
                                    skip = 0;
                                    while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip) != -1)
                                    {
                                        charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip);
                                        if (enemies.Members[charNumber].CanUseHealMagic()
                                            && enemies.Members[charNumber].MaxHitPoints - enemies.Members[charNumber].HitPoints >= 5
                                            && charNumber != enemies.MemberOnTurn)
                                        {
                                            charFound = true;
                                            targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip);
                                        }
                                        skip++;
                                    }
                                    if (!charFound)
                                    {
                                        // 3. get the character with the lowest number of hitpoints
                                        int hp_save = -1;
                                        int charNumber_save = -1;
                                        skip = 0;
                                        while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip) != -1)
                                        {
                                            charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip);
                                            if (hp_save == -1 || hp_save > enemies.Members[charNumber].HitPoints)
                                            {
                                                hp_save = enemies.Members[charNumber].HitPoints;
                                                charNumber_save = charNumber;
                                                targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip);
                                            }
                                            skip++;
                                        }
                                        charNumber = charNumber_save;
                                    }
                                }

                                // heal only if the selected character has a damage of 25% or more
                                if (enemies.Members[charNumber].HitPoints <= enemies.Members[charNumber].MaxHitPoints * .75)
                                {
                                    selectedMagicSpell = enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell;
                                    selectedMagicLevel = selectedMagicSpell.Level;
                                    // if the area is >= 2, check what is the position with which the biggest number of allies is affected
                                    if (selectedMagicSpell.Area[selectedMagicLevel - 1] >= 2)
                                    {
                                        map.EmptyMapMarked();
                                        map.MarkHealableAllies(enemies, enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1], enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]);
                                        numTargetsAttackedOrHealed = 1;
                                        MagicHeal(targetPosition, enemies);
                                        int numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                        int targetPosition_save = targetPosition;
                                        if (map.IsMarked(targetPosition - (int)map.Size.X) && map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition - (int)map.Size.X) != -1)
                                        {
                                            numTargetsAttackedOrHealed = 1;
                                            MagicHeal(targetPosition - (int)map.Size.X, enemies);
                                            if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                            {
                                                targetPosition_save = targetPosition - (int)map.Size.X;
                                                numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                            }
                                        }
                                        if (map.IsMarked(targetPosition - 1) && map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition - 1) != -1)
                                        {
                                            numTargetsAttackedOrHealed = 1;
                                            MagicHeal(targetPosition - 1, enemies);
                                            if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                            {
                                                targetPosition_save = targetPosition - 1;
                                                numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                            }
                                        }
                                        if (map.IsMarked(targetPosition + 1) && map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition + 1) != -1)
                                        {
                                            numTargetsAttackedOrHealed = 1;
                                            MagicHeal(targetPosition + 1, enemies);
                                            if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                            {
                                                targetPosition_save = targetPosition + 1;
                                                numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                            }
                                        }
                                        if (map.IsMarked(targetPosition + (int)map.Size.X) && map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition + (int)map.Size.X) != -1)
                                        {
                                            numTargetsAttackedOrHealed = 1;
                                            MagicHeal(targetPosition + (int)map.Size.X, enemies);
                                            if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                            {
                                                targetPosition_save = targetPosition + (int)map.Size.X;
                                                numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                            }
                                        }
                                        targetPosition = targetPosition_save;
                                        charNumber = map.GetCharacterNumberLocatedInGivenPosition(enemies, targetPosition);
                                    }

                                    // here comes the code to get the position to which the enemy has to move
                                    // this algorithm chooses the closest location to the enemy's current location
                                    backUpPosition = map.CalcPositionWhereToMove(enemies, enemies, charNumber, enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1], enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]);
                                    enemyBattleMove = BattleMoves.ItemMagicHeal;
                                    return new CharacterPointer(CharacterPointer.Sides.CPU_Opponents, charNumber);
                                }
                                break;
                        }
                    }
                }
                selectedItemNumber++;
            }

            return null;
        }

        private CharacterPointer EnterEnemyMoveMode_DefeatOpponentPartyWithOneBlow()
        {
            if (enemies.Members[enemies.MemberOnTurn].CanCastAttackSpell())
            {
                selectedSpellNumber = Character.MAXNUMBER_SPELLS - 1;

                while (selectedSpellNumber >= 0)
                {
                    if (enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber] != null
                        && enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].Type == Spell.Types.Attack)
                    {
                        bool charFound = false;
                        int charNumber = -1;

                        // calculate strongest possible magic level
                        selectedMagicLevel = enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].Level;
                        bool found = false;
                        while (!found && selectedMagicLevel >= 1)
                            if (enemies.Members[enemies.MemberOnTurn].MagicPoints < enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MagicPoints[selectedMagicLevel - 1])
                                selectedMagicLevel--;
                            else
                                found = true;
                        if (found)
                        {
                            // choose enemy to attack
                            map.EmptyMapMarked();
                            if (map.MarkAttackableOpponents(enemies, party, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]))
                            {
                                // check whether there is a hero that can be defeated with one spellcast
                                skip = 0;
                                while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                                {
                                    charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                                    if (party.Members[charNumber].MustSurvive
                                        && party.Members[charNumber].HitPoints <= enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].EffectPoints[selectedMagicLevel - 1])
                                    {
                                        charFound = true;
                                        targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                                    }
                                    skip++;
                                }

                                if (charFound)
                                {
                                    // calculate optimum magic level
                                    int effect_save = party.Members[charNumber].HitPoints - enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].EffectPoints[selectedMagicLevel - 1];
                                    if (effect_save < 0)
                                        effect_save = 0;
                                    found = false;
                                    while (!found && selectedMagicLevel >= 1)
                                    {
                                        int effect = party.Members[charNumber].HitPoints - enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].EffectPoints[selectedMagicLevel - 1];
                                        if (effect < 0)
                                            effect = 0;
                                        map.EmptyMapMarked();
                                        map.MarkAttackableOpponents(enemies, party, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]);
                                        if (enemies.Members[enemies.MemberOnTurn].MagicPoints >= enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MagicPoints[selectedMagicLevel - 1]
                                            && map.IsMarked(party.Members[charNumber].Position)
                                            && effect <= effect_save)
                                        {
                                            effect_save = effect;
                                            selectedMagicLevel--;
                                        }
                                        else
                                        {
                                            selectedMagicLevel++;
                                            found = true;
                                        }
                                    }
                                    if (!found)
                                        selectedMagicLevel = 1;

                                    // is a regular attack possible?
                                    map.EmptyMapMarked();
                                    for (int i = enemies.Members[enemies.MemberOnTurn].MinAttackRange; i <= enemies.Members[enemies.MemberOnTurn].MaxAttackRange; i++)
                                        map.MarkFieldsWithDistance(enemies.Members[enemies.MemberOnTurn].Position, i);
                                    skip = 0;
                                    bool regularAttackPossible = false;
                                    while (!regularAttackPossible && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                                        if (map.GetNextCharacterNumberLocatedInMarkedFields(party, skip) == charNumber)
                                            regularAttackPossible = true;
                                        else
                                            skip++;

                                    map.EmptyMapMarked();
                                    map.MarkAttackableOpponents(enemies, party, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]);

                                    // if a regular attack is possible and at least equally effective as a spellcast, then don't cast the spell
                                    int effectMagic = party.Members[charNumber].HitPoints - enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].EffectPoints[selectedMagicLevel - 1];
                                    if (effectMagic < 0)
                                        effectMagic = 0;
                                    int effectAttack = party.Members[charNumber].HitPoints + party.Members[charNumber].DefensePoints - enemies.Members[enemies.MemberOnTurn].AttackPoints;
                                    if (effectAttack < 0)
                                        effectAttack = 0;

                                    if (!regularAttackPossible || effectMagic < effectAttack)
                                    {
                                        // if the area is >= 2, check what is the position with which the biggest number of opponents is affected
                                        if (enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].Area[selectedMagicLevel - 1] >= 2)
                                        {
                                            map.EmptyMapMarked();
                                            map.MarkAttackableOpponents(enemies, party, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]);
                                            numTargetsAttackedOrHealed = 1;
                                            selectedMagicSpell = enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber];
                                            MagicAttack(targetPosition, party);
                                            int numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                            int targetPosition_save = targetPosition;
                                            if (map.IsMarked(targetPosition - (int)map.Size.X) && map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition - (int)map.Size.X) != -1)
                                            {
                                                numTargetsAttackedOrHealed = 1;
                                                MagicAttack(targetPosition - (int)map.Size.X, party);
                                                if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                                {
                                                    targetPosition_save = targetPosition - (int)map.Size.X;
                                                    numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                                }
                                            }
                                            if (map.IsMarked(targetPosition - 1) && map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition - 1) != -1)
                                            {
                                                numTargetsAttackedOrHealed = 1;
                                                MagicAttack(targetPosition - 1, party);
                                                if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                                {
                                                    targetPosition_save = targetPosition - 1;
                                                    numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                                }
                                            }
                                            if (map.IsMarked(targetPosition + 1) && map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition + 1) != -1)
                                            {
                                                numTargetsAttackedOrHealed = 1;
                                                MagicAttack(targetPosition + 1, party);
                                                if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                                {
                                                    targetPosition_save = targetPosition + 1;
                                                    numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                                }
                                            }
                                            if (map.IsMarked(targetPosition + (int)map.Size.X) && map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition + (int)map.Size.X) != -1)
                                            {
                                                numTargetsAttackedOrHealed = 1;
                                                MagicAttack(targetPosition + (int)map.Size.X, party);
                                                if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                                {
                                                    targetPosition_save = targetPosition + (int)map.Size.X;
                                                    numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                                }
                                            }
                                            targetPosition = targetPosition_save;
                                            charNumber = map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition);
                                        }

                                        // here comes the code to get the position to which the enemy has to move
                                        // this algorithm chooses the closest location to the enemy's current location
                                        backUpPosition = map.CalcPositionWhereToMove(enemies, party, charNumber, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]);
                                        enemyBattleMove = BattleMoves.MagicAttack;
                                        return new CharacterPointer(CharacterPointer.Sides.Player, charNumber);
                                    }
                                }
                            }
                        }
                    }
                    selectedSpellNumber--;
                }
            }

            if (enemies.Members[enemies.MemberOnTurn].HasAttackItem())
            {
                bool charFound = false;
                int charNumber = -1;

                selectedItemNumber = 0;

                while (selectedItemNumber < Character.MAXNUMBER_ITEMS)
                {
                    if (enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber] != null
                        && enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell != null
                        && enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Type == Spell.Types.Attack)
                    {
                        // choose enemy to attack
                        map.EmptyMapMarked();
                        if (map.MarkAttackableOpponents(enemies, party, enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1], enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]))
                        {
                            // check whether there is a hero that can be defeated with one spellcast
                            skip = 0;
                            while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                            {
                                charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                                if (party.Members[charNumber].MustSurvive
                                    && party.Members[charNumber].HitPoints <= enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.EffectPoints[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1])
                                {
                                    charFound = true;
                                    targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                                }
                                skip++;
                            }

                            if (charFound)
                            {
                                // is a regular attack possible?
                                map.EmptyMapMarked();
                                for (int i = enemies.Members[enemies.MemberOnTurn].MinAttackRange; i <= enemies.Members[enemies.MemberOnTurn].MaxAttackRange; i++)
                                    map.MarkFieldsWithDistance(enemies.Members[enemies.MemberOnTurn].Position, i);
                                skip = 0;
                                bool regularAttackPossible = false;
                                while (!regularAttackPossible && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                                    if (map.GetNextCharacterNumberLocatedInMarkedFields(party, skip) == charNumber)
                                        regularAttackPossible = true;
                                    else
                                        skip++;

                                map.EmptyMapMarked();
                                map.MarkAttackableOpponents(enemies, party, enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1], enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]);

                                // if a regular attack is possible and at least equally effective as an item spellcast, then don't use the item
                                int effectMagic = party.Members[charNumber].HitPoints - enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.EffectPoints[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1];
                                if (effectMagic < 0)
                                    effectMagic = 0;
                                int effectAttack = party.Members[charNumber].HitPoints + party.Members[charNumber].DefensePoints - enemies.Members[enemies.MemberOnTurn].AttackPoints;
                                if (effectAttack < 0)
                                    effectAttack = 0;

                                if (!regularAttackPossible || effectMagic < effectAttack)
                                {
                                    selectedMagicSpell = enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell;
                                    selectedMagicLevel = selectedMagicSpell.Level;
                                    // if the area is >= 2, check what is the position with which the biggest number of opponents is affected
                                    if (selectedMagicSpell.Area[selectedMagicLevel - 1] >= 2)
                                    {
                                        map.EmptyMapMarked();
                                        map.MarkAttackableOpponents(enemies, party, enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1], enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]);
                                        numTargetsAttackedOrHealed = 1;
                                        MagicAttack(targetPosition, party);
                                        int numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                        int targetPosition_save = targetPosition;
                                        if (map.IsMarked(targetPosition - (int)map.Size.X) && map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition - (int)map.Size.X) != -1)
                                        {
                                            numTargetsAttackedOrHealed = 1;
                                            MagicAttack(targetPosition - (int)map.Size.X, party);
                                            if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                            {
                                                targetPosition_save = targetPosition - (int)map.Size.X;
                                                numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                            }
                                        }
                                        if (map.IsMarked(targetPosition - 1) && map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition - 1) != -1)
                                        {
                                            numTargetsAttackedOrHealed = 1;
                                            MagicAttack(targetPosition - 1, party);
                                            if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                            {
                                                targetPosition_save = targetPosition - 1;
                                                numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                            }
                                        }
                                        if (map.IsMarked(targetPosition + 1) && map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition + 1) != -1)
                                        {
                                            numTargetsAttackedOrHealed = 1;
                                            MagicAttack(targetPosition + 1, party);
                                            if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                            {
                                                targetPosition_save = targetPosition + 1;
                                                numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                            }
                                        }
                                        if (map.IsMarked(targetPosition + (int)map.Size.X) && map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition + (int)map.Size.X) != -1)
                                        {
                                            numTargetsAttackedOrHealed = 1;
                                            MagicAttack(targetPosition + (int)map.Size.X, party);
                                            if (numTargetsAttackedOrHealed > numTargetsAttackedOrHealed_save)
                                            {
                                                targetPosition_save = targetPosition + (int)map.Size.X;
                                                numTargetsAttackedOrHealed_save = numTargetsAttackedOrHealed;
                                            }
                                        }
                                        targetPosition = targetPosition_save;
                                        charNumber = map.GetCharacterNumberLocatedInGivenPosition(party, targetPosition);
                                    }

                                    // here comes the code to get the position to which the enemy has to move
                                    // this algorithm chooses the closest location to the enemy's current location
                                    backUpPosition = map.CalcPositionWhereToMove(enemies, party, charNumber, enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1], enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]);
                                    enemyBattleMove = BattleMoves.ItemMagicAttack;
                                    return new CharacterPointer(CharacterPointer.Sides.Player, charNumber);
                                }
                            }
                        }
                    }
                    selectedItemNumber++;
                }
            }

            if (map.MarkAttackableOpponents(enemies, party, enemies.Members[enemies.MemberOnTurn].MinAttackRange, enemies.Members[enemies.MemberOnTurn].MaxAttackRange))
            {
                bool charFound = false;
                int charNumber = -1;
                // check whether there is a hero that can be defeated with one blow
                skip = 0;
                while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                {
                    charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                    if (party.Members[charNumber].MustSurvive
                        && party.Members[charNumber].HitPoints <= enemies.Members[enemies.MemberOnTurn].AttackPoints - party.Members[charNumber].DefensePoints)
                    {
                        charFound = true;
                        targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                    }
                    skip++;
                }

                if (charFound)
                {
                    // here comes the code to get the position to which the enemy has to move
                    // this algorithm chooses the closest location to the enemy's current location
                    backUpPosition = map.CalcPositionWhereToMove(enemies, party, charNumber, enemies.Members[enemies.MemberOnTurn].MinAttackRange, enemies.Members[enemies.MemberOnTurn].MaxAttackRange);
                    enemyBattleMove = BattleMoves.Attack;
                    return new CharacterPointer(CharacterPointer.Sides.Player, charNumber);
                }
            }

            return null;
        }

        private void EnterEnemyMoveMode_Stay()
        {
            bool charFound = false;
            int charNumber = -1;

            backUpPosition = (int)enemies.Members[enemies.MemberOnTurn].Position;

            if (!enemies.Members[enemies.MemberOnTurn].MustSurvive
                && enemies.Members[enemies.MemberOnTurn].CanCastHealSpell())
            {
                // calculate how far the character can go within two turns
                map.EmptyMapMarked();
                map.MarkViable();
                map.CalcViableFromMarked(enemies.Members[enemies.MemberOnTurn].MovePoints, enemies.Members[enemies.MemberOnTurn].Flying, party);

                selectedSpellNumber = Character.MAXNUMBER_SPELLS - 1;

                while (selectedSpellNumber >= 0)
                {
                    if (enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber] != null
                        && enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].Type == Spell.Types.Heal)
                    {
                        // calculate strongest possible magic level
                        selectedMagicLevel = enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].Level;
                        bool found = false;
                        while (!found && selectedMagicLevel >= 1)
                            if (enemies.Members[enemies.MemberOnTurn].MagicPoints < enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MagicPoints[selectedMagicLevel - 1])
                                selectedMagicLevel--;
                            else
                                found = true;
                        if (found)
                        {
                            map.EmptyMapMarked();
                            if (map.MarkHealableAllies(enemies, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]))
                            {
                                // check whether there is a "must survive" ally
                                skip = 0;
                                while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip) != -1)
                                {
                                    charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip);
                                    if (enemies.Members[charNumber].MustSurvive)
                                    {
                                        charFound = true;
                                        targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip);
                                    }
                                    skip++;
                                }
                            }
                        }

                        if (charFound)
                        {
                            // here comes the code to get the position to which the enemy has to move
                            // this algorithm chooses the closest location to the enemy's current location
                            backUpPosition = map.CalcPositionWhereToMove(enemies, enemies, charNumber, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]);
                            map.CalcBestPath((int)enemies.Members[enemies.MemberOnTurn].Position, (int)backUpPosition, enemies.Members[enemies.MemberOnTurn].MovePoints * 2);
                            map.EmptyMapMarked();
                            map.EmptyMapViable();
                            map.CalcViable(enemies.Members[enemies.MemberOnTurn].Position, enemies.Members[enemies.MemberOnTurn].MovePoints, enemies.Members[enemies.MemberOnTurn].Flying, party);
                            backUpPosition = map.FollowBestPath((int)enemies.Members[enemies.MemberOnTurn].Position, enemies);
                        }
                    }
                    selectedSpellNumber--;
                }
            }

            if (!enemies.Members[enemies.MemberOnTurn].MustSurvive 
                && (!enemies.Members[enemies.MemberOnTurn].CanCastHealSpell()
                    || !charFound))
            {
                // calculate how far the character can go within two turns
                map.EmptyMapMarked();
                map.MarkViable();
                map.CalcViableFromMarked(enemies.Members[enemies.MemberOnTurn].MovePoints, enemies.Members[enemies.MemberOnTurn].Flying, party);
                map.EmptyMapMarked();
                if (map.MarkAttackableOpponents(enemies, party, enemies.Members[enemies.MemberOnTurn].MinAttackRange, enemies.Members[enemies.MemberOnTurn].MaxAttackRange))
                {
                    // 1. check whether there is a hero
                    skip = 0;
                    while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                    {
                        charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                        if (party.Members[charNumber].MustSurvive)
                        {
                            charFound = true;
                            targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                        }
                        skip++;
                    }
                    if (!charFound)
                    {
                        // 2. check whether there is a healer with enough MP or somebody who can use a heal item
                        skip = 0;
                        while (!charFound && map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                        {
                            charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                            if (party.Members[charNumber].CanUseHealMagic())
                            {
                                charFound = true;
                                targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                            }
                            skip++;
                        }
                    }
                }

                if (charFound)
                {
                    // here comes the code to get the position to which the enemy has to move
                    // this algorithm chooses the closest location to the enemy's current location
                    backUpPosition = map.CalcPositionWhereToMove(enemies, party, charNumber, enemies.Members[enemies.MemberOnTurn].MinAttackRange, enemies.Members[enemies.MemberOnTurn].MaxAttackRange);
                    map.CalcBestPath((int)enemies.Members[enemies.MemberOnTurn].Position, (int)backUpPosition, enemies.Members[enemies.MemberOnTurn].MovePoints * 2);
                    map.EmptyMapMarked();
                    map.EmptyMapViable();
                    map.CalcViable(enemies.Members[enemies.MemberOnTurn].Position, enemies.Members[enemies.MemberOnTurn].MovePoints, enemies.Members[enemies.MemberOnTurn].Flying, party);
                    backUpPosition = map.FollowBestPath((int)enemies.Members[enemies.MemberOnTurn].Position, enemies);
                }
            }
        }

        private CharacterPointer EnterEnemyMoveMode_AttackConfused()
        {
            if (map.MarkAttackableOpponents(enemies, enemies, enemies.Members[enemies.MemberOnTurn].MinAttackRange, enemies.Members[enemies.MemberOnTurn].MaxAttackRange))
            {
                skip = 0;
                int charNumber = -1;
                do
                {
                    charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip);
                    targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip);
                    skip++;
                } while (charNumber == enemies.MemberOnTurn);

                if (charNumber != -1)
                {
                    // here comes the code to get the position to which the enemy has to move
                    // this algorithm chooses the closest location to the enemy's current location
                    backUpPosition = map.CalcPositionWhereToMove(enemies, enemies, charNumber, enemies.Members[enemies.MemberOnTurn].MinAttackRange, enemies.Members[enemies.MemberOnTurn].MaxAttackRange);
                    enemyBattleMove = BattleMoves.Attack;
                    return new CharacterPointer(CharacterPointer.Sides.CPU_Opponents, charNumber);
                }
            }
            return null;
        }

        // ******************************

        private CharacterPointer EnterEnemyMoveMode_MagicAttackConfused()
        {
            // *** this algorithm doesn't support muddle, dispel, desoul, slow yet

            selectedSpellNumber = Character.MAXNUMBER_SPELLS - 1;

            while (selectedSpellNumber >= 0)
            {
                if (enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber] != null
                    && enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].Type == Spell.Types.Attack)
                {
                    // calculate strongest possible magic level
                    selectedMagicLevel = enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].Level;
                    bool found = false;
                    while (!found && selectedMagicLevel >= 1)
                        if (enemies.Members[enemies.MemberOnTurn].MagicPoints < enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MagicPoints[selectedMagicLevel - 1])
                            selectedMagicLevel--;
                        else
                            found = true;
                    if (found)
                    {
                        map.EmptyMapMarked();
                        if (map.MarkAttackableOpponents(enemies, enemies, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]))
                        {
                            skip = 0;
                            int charNumber = -1;
                            do
                            {
                                charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip);
                                targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip);
                                skip++;
                            } while (charNumber == enemies.MemberOnTurn);

                            if (charNumber != -1)
                            {
                                // calculate optimum magic level
                                int effect_save = enemies.Members[charNumber].HitPoints - enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].EffectPoints[selectedMagicLevel - 1];
                                if (effect_save < 0)
                                    effect_save = 0;
                                found = false;
                                while (!found && selectedMagicLevel >= 1)
                                {
                                    int effect = enemies.Members[charNumber].HitPoints - enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].EffectPoints[selectedMagicLevel - 1];
                                    if (effect < 0)
                                        effect = 0;
                                    map.EmptyMapMarked();
                                    map.MarkAttackableOpponents(enemies, enemies, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]);
                                    if (enemies.Members[enemies.MemberOnTurn].MagicPoints >= enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MagicPoints[selectedMagicLevel - 1]
                                        && map.IsMarked(enemies.Members[charNumber].Position)
                                        && effect <= effect_save)
                                    {
                                        effect_save = effect;
                                        selectedMagicLevel--;
                                    }
                                    else
                                    {
                                        selectedMagicLevel++;
                                        found = true;
                                    }
                                }
                                if (!found)
                                    selectedMagicLevel = 1;

                                // is a regular attack possible?
                                map.EmptyMapMarked();
                                for (int i = enemies.Members[enemies.MemberOnTurn].MinAttackRange; i <= enemies.Members[enemies.MemberOnTurn].MaxAttackRange; i++)
                                    map.MarkFieldsWithDistance(enemies.Members[enemies.MemberOnTurn].Position, i);
                                skip = 0;
                                bool regularAttackPossible = false;
                                while (!regularAttackPossible && map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip) != -1)
                                    if (map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip) == charNumber)
                                        regularAttackPossible = true;
                                    else
                                        skip++;

                                map.EmptyMapMarked();
                                map.MarkAttackableOpponents(enemies, enemies, enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MinRange[selectedMagicLevel - 1], enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].MaxRange[selectedMagicLevel - 1]);

                                // if a regular attack is possible and at least equally effective as a spellcast, then don't cast the spell
                                int effectMagic = enemies.Members[charNumber].HitPoints - enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber].EffectPoints[selectedMagicLevel - 1];
                                if (effectMagic < 0)
                                    effectMagic = 0;
                                int effectAttack = enemies.Members[charNumber].HitPoints + enemies.Members[charNumber].DefensePoints - enemies.Members[enemies.MemberOnTurn].AttackPoints;
                                if (effectAttack < 0)
                                    effectAttack = 0;

                                if (!regularAttackPossible || effectMagic < effectAttack)
                                {
                                    // here comes the code to get the position to which the enemy has to move
                                    // this algorithm chooses the closest location to the enemy's current location
                                    backUpPosition = map.CalcPositionWhereToMove(enemies, enemies, charNumber, enemies.Members[enemies.MemberOnTurn].MinAttackRange, enemies.Members[enemies.MemberOnTurn].MaxAttackRange);
                                    enemyBattleMove = BattleMoves.MagicAttack;
                                    return new CharacterPointer(CharacterPointer.Sides.CPU_Opponents, charNumber);
                                }
                            }
                        }
                    }
                }
                selectedSpellNumber--;
            }

            return null;
        }

        // ******************************

        private CharacterPointer EnterEnemyMoveMode_ItemMagicAttackConfused()
        {
            // *** this algorithm doesn't support muddle, dispel, desoul, slow yet

            selectedItemNumber = 0;

            while (selectedItemNumber < Character.MAXNUMBER_ITEMS)
            {
                if (enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber] != null
                    && enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell != null
                    && enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Type == Spell.Types.Attack)
                {
                    map.EmptyMapMarked();
                    if (map.MarkAttackableOpponents(enemies, enemies, enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1], enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]))
                    {
                        skip = 0;
                        int charNumber = -1;
                        do
                        {
                            charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip);
                            targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip);
                            skip++;
                        } while (charNumber == enemies.MemberOnTurn);

                        // is a regular attack possible?
                        map.EmptyMapMarked();
                        for (int i = enemies.Members[enemies.MemberOnTurn].MinAttackRange; i <= enemies.Members[enemies.MemberOnTurn].MaxAttackRange; i++)
                            map.MarkFieldsWithDistance(enemies.Members[enemies.MemberOnTurn].Position, i);
                        skip = 0;
                        bool regularAttackPossible = false;
                        while (!regularAttackPossible && map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip) != -1)
                            if (map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip) == charNumber)
                                regularAttackPossible = true;
                            else
                                skip++;

                        map.EmptyMapMarked();
                        map.MarkAttackableOpponents(enemies, enemies, enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MinRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1], enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.MaxRange[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1]);

                        // if a regular attack is possible and at least equally effective as an item spellcast, then don't use the item
                        int effectMagic = enemies.Members[charNumber].HitPoints - enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.EffectPoints[enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell.Level - 1];
                        if (effectMagic < 0)
                            effectMagic = 0;
                        int effectAttack = enemies.Members[charNumber].HitPoints + enemies.Members[charNumber].DefensePoints - enemies.Members[enemies.MemberOnTurn].AttackPoints;
                        if (effectAttack < 0)
                            effectAttack = 0;

                        if (!regularAttackPossible || effectMagic < effectAttack)
                        {
                            // here comes the code to get the position to which the enemy has to move
                            // this algorithm chooses the closest location to the enemy's current location
                            backUpPosition = map.CalcPositionWhereToMove(enemies, enemies, charNumber, enemies.Members[enemies.MemberOnTurn].MinAttackRange, enemies.Members[enemies.MemberOnTurn].MaxAttackRange);
                            enemyBattleMove = BattleMoves.Attack;
                            return new CharacterPointer(CharacterPointer.Sides.CPU_Opponents, charNumber);
                        }
                    }
                }
                selectedItemNumber++;
            }

            return null;
        }

        private void EnterEnemyMoveMode_StayConfused()
        {
            backUpPosition = (int)enemies.Members[enemies.MemberOnTurn].Position;

            // calculate how far the character can go within two turns
            map.EmptyMapMarked();
            map.MarkViable();
            map.CalcViableFromMarked(enemies.Members[enemies.MemberOnTurn].MovePoints, enemies.Members[enemies.MemberOnTurn].Flying, party);
            if (map.MarkAttackableOpponents(enemies, enemies, enemies.Members[enemies.MemberOnTurn].MinAttackRange, enemies.Members[enemies.MemberOnTurn].MaxAttackRange))
            {
                skip = 0;
                int charNumber = -1;
                do
                {
                    charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(enemies, skip);
                    targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(enemies, skip);
                    skip++;
                } while (charNumber == enemies.MemberOnTurn);

                if (charNumber != -1)
                {
                    // here comes the code to get the position to which the enemy has to move
                    // this algorithm chooses the closest location to the enemy's current location
                    backUpPosition = map.CalcPositionWhereToMove(enemies, enemies, charNumber, enemies.Members[enemies.MemberOnTurn].MinAttackRange, enemies.Members[enemies.MemberOnTurn].MaxAttackRange);
                    map.CalcBestPath((int)enemies.Members[enemies.MemberOnTurn].Position, (int)backUpPosition, enemies.Members[enemies.MemberOnTurn].MovePoints * 2);
                    map.EmptyMapMarked();
                    map.EmptyMapViable();
                    map.CalcViable(enemies.Members[enemies.MemberOnTurn].Position, enemies.Members[enemies.MemberOnTurn].MovePoints, enemies.Members[enemies.MemberOnTurn].Flying, party);
                    backUpPosition = map.FollowBestPath((int)enemies.Members[enemies.MemberOnTurn].Position, enemies);
                }
            }
        }

        private void EnterEnemyMovingMode()
        {
            GameMode = GameModes.EnemyMovingMode;

            delayPosition = map.CalcPosition(enemies.Members[enemies.MemberOnTurn].Position);
            delayPosition.X *= Map.TILESIZEX;
            delayPosition.Y *= Map.TILESIZEY;

            oldMapPosition = map.Position;

            if (!map.IsVisible(targetPosition, map.Position))
            {
                Vector2 newMapPosition = map.CalcClosestMapPositionSoThatVisible(targetPosition);

                if (newMapPosition.X > map.Position.X)
                {
                    map.Position.X++;
                    map.Offset.X = Map.TILESIZEX;
                }
                else if (newMapPosition.X < map.Position.X)
                {
                    map.Position.X--;
                    map.Offset.X = -Map.TILESIZEX;
                }

                if (newMapPosition.Y > map.Position.Y)
                {
                    map.Position.Y++;
                    map.Offset.Y = Map.TILESIZEY;
                }
                else if (newMapPosition.Y < map.Position.Y)
                {
                    map.Position.Y--;
                    map.Offset.Y = -Map.TILESIZEY;
                }
            }
        }

        private void EnterEnemyBattleMode()
        {
            GameMode = GameModes.EnemyBattleMode;
            battleModeState = 0;

            map.EmptyMapViable();
            map.EmptyMapMarked();

            switch (enemyBattleMove)
            {
                case BattleMoves.Attack:
                    for (int i = enemies.Members[enemies.MemberOnTurn].MinAttackRange; i <= enemies.Members[enemies.MemberOnTurn].MaxAttackRange; i++)
                        map.MarkFieldsWithDistance(enemies.Members[enemies.MemberOnTurn].Position, i);
                    break;

                case BattleMoves.MagicAttack:
                case BattleMoves.MagicHeal:
                    selectedMagicSpell = enemies.Members[enemies.MemberOnTurn].MagicSpells[selectedSpellNumber];
                    break;

                case BattleMoves.ItemMagicAttack:
                case BattleMoves.ItemMagicHeal:
                    selectedMagicSpell = enemies.Members[enemies.MemberOnTurn].Items[selectedItemNumber].MagicSpell;
                    selectedMagicLevel = selectedMagicSpell.Level;
                    break;
            }

            targetsAttackedOrHealed[0] = target;
            numTargetsAttackedOrHealed = 1;

            switch (enemyBattleMove)
            {
                case BattleMoves.MagicAttack:
                case BattleMoves.ItemMagicAttack:
                    for (int i = selectedMagicSpell.MinRange[selectedMagicLevel - 1]; i <= selectedMagicSpell.MaxRange[selectedMagicLevel - 1]; i++)
                        map.MarkFieldsWithDistance(enemies.Members[enemies.MemberOnTurn].Position, i);
                    MagicAttack(targetPosition, party);
                    break;

                case BattleMoves.MagicHeal:
                case BattleMoves.ItemMagicHeal:
                    for (int i = selectedMagicSpell.MinRange[selectedMagicLevel - 1]; i <= selectedMagicSpell.MaxRange[selectedMagicLevel - 1]; i++)
                        map.MarkFieldsWithDistance(enemies.Members[enemies.MemberOnTurn].Position, i);
                    MagicHeal(targetPosition, enemies);
                    break;
            }

            currentTargetAttackedOrHealed = 0;        
        }

        private void EnterEnemyBattleDisplayTextMessageMode()
        {
            positionInTextMessage.Row = 0;
            positionInTextMessage.Column = 0;
            textMessageDelay = 0;
            // 2024.12.02 introduced constants
            textMessageBox.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - textMessageBox.Size.X) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 30 - textMessageBox.Size.Y);
            arrowforward.Position = textMessageBox.Position + textMessageBox.Size - new Vector2(34, 30);
            //keyRelieved = false;
            textMessageContinueFlag = false;
            GameMode = GameModes.EnemyBattleDisplayTextMessageMode;
        }

        private void EnterEnemyAttackMode()
        {
            oldSelectionBarPosition = map.CalcPosition(enemies.Members[enemies.MemberOnTurn].Position);
            selectionBar.Position = map.CalcPosition(targetPosition);

            if (map.CalcPosition(enemies.Members[enemies.MemberOnTurn].Position).Y < selectionBar.Position.Y)
                enemies.Members[enemies.MemberOnTurn].LookDown();
            else if (map.CalcPosition(enemies.Members[enemies.MemberOnTurn].Position).Y > selectionBar.Position.Y)
                enemies.Members[enemies.MemberOnTurn].LookUp();
            else if (map.CalcPosition(enemies.Members[enemies.MemberOnTurn].Position).X < selectionBar.Position.X)
                enemies.Members[enemies.MemberOnTurn].LookRight();
            else if (map.CalcPosition(enemies.Members[enemies.MemberOnTurn].Position).X > selectionBar.Position.X)
                enemies.Members[enemies.MemberOnTurn].LookLeft();
            
            oldMapPosition = map.Position;

            if (selectionBar.Position.Y < 3)
            {
                map.Position.Y -= 3 - selectionBar.Position.Y;
                selectionBar.Position.Y = 3;
                while (!map.InBoundaries(map.Position))
                {
                    map.Position.Y++;
                    selectionBar.Position.Y--;
                }
            }

            GameMode = GameModes.EnemyAttackMode;
        }

        private void EnterHealMenuMode()
        {
            GameMode = GameModes.HealMenuMode;
            oldSelectionBarPosition = map.CalcPosition(party.Members[party.MemberOnTurn].Position);
            // select party member in proximity with the largest defect
            skip = 0;
            int bestSkip = 0;
            int bestSkipLostHP = 0;
            while (map.GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
            {
                int tempCharacterNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                if (party.Members[tempCharacterNumber].MaxHitPoints - party.Members[tempCharacterNumber].HitPoints > bestSkipLostHP)
                {
                    bestSkip = skip;
                    bestSkipLostHP = party.Members[tempCharacterNumber].MaxHitPoints - party.Members[tempCharacterNumber].HitPoints;
                }
                skip++;
            }
            skip = bestSkip;
            UpdateInput_HealMenuMode_CursorKey_Common();
        }

        private void EnterEndTurnOfCharacterAndNextCharacterMode()
        {
            GameMode = GameModes.EndTurnOfCharacterAndNextCharacterMode;
            endTurnOfCharacterAndNextCharacterModeState = EndTurnOfCharacterAndNextCharacterModeStates.Poisoned;
            map.EmptyMapMarked();
            if (party.IsDefeated())
                EnterBattleLost1Mode();
            else if (enemies.IsDefeated())
                EnterBattleWonMode();
        }

        private void EnterBattleLost1Mode()
        {
            battleWon = false;
            int tempNumber = party.GetNumberOfExhaustedLeader();
            if (tempNumber != -1)
            {
                textMessage[0] = party.Members[tempNumber].Name + " is";
                textMessage[1] = "exhausted.";
                textMessageLength = 2;
                returnGameMode = GameModes.BattleLost2Mode;
                waitForKeyPress = true;
                moveOut = true;
                EnterDisplayTextMessageMode();
            }
            else
                EnterBattleLost2Mode();
        }

        private void EnterBattleLost2Mode()
        {
            if (gold / 2 >= ENOUGH_MONEY_TO_CONTINUE_ONCE)
                gold /= 2;
            else if (gold >= ENOUGH_MONEY_TO_CONTINUE_ONCE)
                gold -= ENOUGH_MONEY_TO_CONTINUE_ONCE;
            else
            {
                map = null;
                if (File.Exists("savegame.sav"))
                    File.Delete("savegame.sav");
                textMessage[0] = "You do not have enough gold to";
                textMessage[1] = "continue. GAME OVER.";
                textMessageLength = 2;
                returnGameMode = GameModes.TitleScreenMode;
                waitForKeyPress = true;
                moveOut = true;
                EnterDisplayTextMessageMode();
                return;
            }

            returnGameMode = GameModes.EndOfBigBattleMode;
            EnterBattleFieldFadeOutMode();
        }

        private void EnterBattleWonMode()
        {
            battleWon = true;
            returnGameMode = GameModes.EndOfBigBattleMode;
            EnterBattleFieldFadeOutMode();
        }

        private void EnterPlayerAutoMoveMode()
        {
            map.EmptyMapViable();
            map.EmptyMapMarked();
            if (party.Members[party.MemberOnTurn].Asleep > 0)
            {
                party.Members[party.MemberOnTurn].Asleep--;
                if (party.Members[party.MemberOnTurn].Asleep == 0)
                    textMessage[0] = party.Members[party.MemberOnTurn].Name + " has awakened.";
                else
                    textMessage[0] = party.Members[party.MemberOnTurn].Name + " fell asleep.";
                textMessageLength = 1;
                returnGameMode = GameModes.NextCharacterAfterSleepMode;
                waitForKeyPress = true;
                moveOut = true;
                EnterDisplayTextMessageMode();
            }
            else if (party.Members[party.MemberOnTurn].Stunned > 0)
            {
                party.Members[party.MemberOnTurn].Stunned--;
                // *** check if these messages correspond to the original game
                if (party.Members[party.MemberOnTurn].Stunned == 0)
                    textMessage[0] = party.Members[party.MemberOnTurn].Name + " is no longer stunned.";
                else
                    textMessage[0] = party.Members[party.MemberOnTurn].Name + " is stunned.";
                textMessageLength = 1;
                returnGameMode = GameModes.NextCharacterAfterSleepMode;
                waitForKeyPress = true;
                moveOut = true;
                EnterDisplayTextMessageMode();
            }
            else
            {
                GameMode = GameModes.PlayerAutoMoveMode;
                map.CalcViable(party.Members[party.MemberOnTurn].Position, party.Members[party.MemberOnTurn].MovePoints, party.Members[party.MemberOnTurn].Flying, enemies);
                battleMenu.CurrentState = BattleMenu.States.StaySelected1;
                if (party.Members[party.MemberOnTurn].Confused > 0)
                {
                    target = EnterPlayerAutoMoveMode_AttackConfused();
                    if (battleMenu.CurrentState == BattleMenu.States.StaySelected1)
                        EnterPlayerAutoMoveMode_StayConfused();
                }
                else
                {
                    // currently automated moving is supported only if the party member is confused
                    backUpPosition = (int)party.Members[party.MemberOnTurn].Position;
                }
                map.CalcBestPath((int)party.Members[party.MemberOnTurn].Position, (int)backUpPosition, party.Members[party.MemberOnTurn].MovePoints);
                currentStepNumber = 0;
            }
            map.EmptyMapMarked();
            map.EmptyMapViable();
            map.CalcViable(party.Members[party.MemberOnTurn].Position, party.Members[party.MemberOnTurn].MovePoints, party.Members[party.MemberOnTurn].Flying, enemies);
        }

        private CharacterPointer EnterPlayerAutoMoveMode_AttackConfused()
        {
            if (map.MarkAttackableOpponents(party, party, party.Members[party.MemberOnTurn].MinAttackRange, party.Members[party.MemberOnTurn].MaxAttackRange))
            {
                skip = 0;
                int charNumber = -1;
                do
                {
                    charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                    targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                    skip++;
                } while (charNumber == party.MemberOnTurn);

                if (charNumber != -1)
                {
                    // here comes the code to get the position to which the enemy has to move
                    // this algorithm chooses the closest location to the enemy's current location
                    backUpPosition = map.CalcPositionWhereToMove(party, party, charNumber, party.Members[party.MemberOnTurn].MinAttackRange, party.Members[party.MemberOnTurn].MaxAttackRange);
                    battleMenu.CurrentState = BattleMenu.States.AttackSelected1;
                    return new CharacterPointer(CharacterPointer.Sides.Player, charNumber);
                }
            }
            return null;
        }

        private void EnterPlayerAutoMoveMode_StayConfused()
        {
            backUpPosition = (int)party.Members[party.MemberOnTurn].Position;

            // calculate how far the character can go within two turns
            map.EmptyMapMarked();
            map.MarkViable();
            map.CalcViableFromMarked(party.Members[party.MemberOnTurn].MovePoints, party.Members[party.MemberOnTurn].Flying, enemies);
            if (map.MarkAttackableOpponents(party, party, party.Members[party.MemberOnTurn].MinAttackRange, party.Members[party.MemberOnTurn].MaxAttackRange))
            {
                skip = 0;
                int charNumber = -1;
                do
                {
                    charNumber = map.GetNextCharacterNumberLocatedInMarkedFields(party, skip);
                    targetPosition = map.GetNextCharacterPositionLocatedInMarkedFields(party, skip);
                    skip++;
                } while (charNumber == party.MemberOnTurn);

                if (charNumber != -1)
                {
                    // here comes the code to get the position to which the enemy has to move
                    // this algorithm chooses the closest location to the enemy's current location
                    backUpPosition = map.CalcPositionWhereToMove(party, party, charNumber, party.Members[party.MemberOnTurn].MinAttackRange, party.Members[party.MemberOnTurn].MaxAttackRange);
                    map.CalcBestPath((int)party.Members[party.MemberOnTurn].Position, (int)backUpPosition, party.Members[party.MemberOnTurn].MovePoints * 2);
                    map.EmptyMapMarked();
                    map.EmptyMapViable();
                    map.CalcViable(party.Members[party.MemberOnTurn].Position, party.Members[party.MemberOnTurn].MovePoints, party.Members[party.MemberOnTurn].Flying, enemies);
                    backUpPosition = map.FollowBestPath((int)party.Members[party.MemberOnTurn].Position, party);
                }
            }
        }

        private void EnterPlayerAutoAttackMode()
        {
            GameMode = GameModes.PlayerAutoAttackMode;
            oldSelectionBarPosition = map.CalcPosition(party.Members[party.MemberOnTurn].Position);
            selectionBar.Position = map.CalcPosition(targetPosition);

            if (map.CalcPosition(party.Members[party.MemberOnTurn].Position).Y < selectionBar.Position.Y)
                party.Members[party.MemberOnTurn].LookDown();
            else if (map.CalcPosition(party.Members[party.MemberOnTurn].Position).Y > selectionBar.Position.Y)
                party.Members[party.MemberOnTurn].LookUp();
            else if (map.CalcPosition(party.Members[party.MemberOnTurn].Position).X < selectionBar.Position.X)
                party.Members[party.MemberOnTurn].LookRight();
            else if (map.CalcPosition(party.Members[party.MemberOnTurn].Position).X > selectionBar.Position.X)
                party.Members[party.MemberOnTurn].LookLeft();

            oldMapPosition = map.Position;

            if (selectionBar.Position.Y < 3)
            {
                map.Position.Y -= 3 - selectionBar.Position.Y;
                selectionBar.Position.Y = 3;
                while (!map.InBoundaries(map.Position))
                {
                    map.Position.Y++;
                    selectionBar.Position.Y--;
                }
            }
        }

        private void EnterEgressMode()
        {
            battleWon = false;
            returnGameMode = GameModes.EndOfBigBattleMode;
            EnterBattleFieldFadeOutMode();
        }

        private void EnterBattleFieldFadeOutMode()
        {
            GameMode = GameModes.BattleFieldFadeOutMode;
            fadingState = 255;
            fadingSpeed = 3;
        }

        private void EnterBattleFieldFadeInMode()
        {
            GameMode = GameModes.BattleFieldFadeInMode;
            fadingState = 0;
            fadingSpeed = 3;
        }

        private void LoadGame()
        {
            byte[] saveState = new byte[4096];

            if (File.Exists("savegame.sav"))
            {
                saveState = File.ReadAllBytes("savegame.sav");
                progress = (int)saveState[156 + saveState[156]];
                gold = 256 * saveState[156 + saveState[156] + 2] + saveState[156 + saveState[156] + 1];

                // 2015.06.27 Also store character levels in savegame
                hero.Level = saveState[156 + saveState[156] + 3]; 
                healer.Level = saveState[156 + saveState[156] + 4]; 
                warrior.Level = saveState[156 + saveState[156] + 5]; 
                archer.Level = saveState[156 + saveState[156] + 6]; 
                knight.Level = saveState[156 + saveState[156] + 7];
                mage.Level = saveState[156 + saveState[156] + 8]; 
            }
            else
            {
                progress = 0;
                gold = ENOUGH_MONEY_TO_CONTINUE_ONCE;
            }
        }

        private void SaveGame()
        {
            byte [] saveState = new byte[4096];
            int i, j;
            Random random = new Random();

            for (i = 0; i <= 156; i++)
                saveState [i] = (byte) random.Next (0, 255);
            if (saveState[156] == 0)
                saveState[156] = 1;
            for (j = saveState [156]; j > 0; j--)
                saveState[i + j] = (byte) random.Next(0, 255);
            saveState[156 + saveState[156]] = (byte) progress;
            saveState[156 + saveState[156] + 1] = (byte) (gold % 256);
            saveState[156 + saveState[156] + 2] = (byte) ((gold / 256) % 256);

            // 2015.06.27 Also store character levels in savegame
            saveState[156 + saveState[156] + 3] = (byte)hero.Level;
            saveState[156 + saveState[156] + 4] = (byte)healer.Level;
            saveState[156 + saveState[156] + 5] = (byte)warrior.Level;
            saveState[156 + saveState[156] + 6] = (byte)archer.Level;
            saveState[156 + saveState[156] + 7] = (byte)knight.Level;
            saveState[156 + saveState[156] + 8] = (byte)mage.Level;

            for (i = 156 + saveState[156] + 9; i < 4096; i++)
                saveState[i] = (byte) random.Next(0, 255);

            File.WriteAllBytes("savegame.sav", saveState);
        }

        private void EnterInitializeBattle1Mode()
        {
            SaveGame();

            // only in this game
            party.RegenerateFully();

            switch (progress)
            {
                case 0:
                    map = new Map(@"Content\\new_map01.map");
                    break;

                case 1:
                    map = new Map(@"Content\\new_map02.map");
                    break;

                case 2:
                    map = new Map(@"Content\\new_map03.map");
                    break;

                case 3:
                    map = new Map(@"Content\\new_map04.map");
                    break;

                case 4:
                    map = new Map(@"Content\\new_map05.map");
                    break;

                case 5:
                    map = new Map(@"Content\\new_map06.map");
                    break;

                case 6:
                    map = new Map(@"Content\\new_map07.map");
                    break;

                case 7:
                    map = new Map(@"Content\\new_map08.map");
                    break;

                case 8:
                    map = new Map(@"Content\\new_map09.map");
                    break;

                case 9:
                    map = new Map(@"Content\\new_map10.map");
                    break;

                case 10:
                    GameMode = GameModes.TitleScreenMode;
                    return;
            }

            gameTimeAtLastBlink = 0;
            blinkDelayReached = false;

            for (int i = 0; i < party.NumPartyMembers; i++)
                party.Members[i].Position = map.GetStartingPositionOfPlayerPartyMember(i);

            enemies = new Party(map.GetNumberOfEnemies(), CharacterPointer.Sides.CPU_Opponents);
            for (int i = 0; i < enemies.NumPartyMembers; i++)
            {
                enemies.Members[i] = new Character(map.GetEnemyId(i), CharacterPointer.Sides.CPU_Opponents);
                enemies.Members[i].Position = map.GetStartingPositionOfEnemyPartyMember(i);
            }

            returnGameMode = GameModes.InitializeBattle2Mode;
            EnterBattleFieldFadeInMode();
        }

        private void EnterInitializeBattle2Mode()
        {
            turn = 0;
            memberMenuState_selectedMemberNumber = 0;
            memberMenuState_firstMemberNumberToDisplay = 0;
            memberMenuState_displayDetails = false;

            ResetMapPositionPlayerMoveMode();
            ResetMapPositionEnemyMoveMode();

            characterPointer = CalculateCharacterPointersForThisBattle(new Party[] { party, enemies });
            if (characterPointer.BelongsToSide == CharacterPointer.Sides.Player)
                selectionBar.Position = map.CalcPosition(party.Members[characterPointer.WhichOne].Position);
            else
                selectionBar.Position = map.CalcPosition(enemies.Members[characterPointer.WhichOne].Position);
            NextCharacter();
        }

        private void EnterEndOfBigBattleMode()
        {
            GameMode = GameModes.EndOfBigBattleMode;
            // In this game, the exhausted are automatically healed after each battle.
            // 2024.12.02 The healing was done at another place, which was wrong because it caused the "fading out bug" (dead characters appeared again when the battle was fading out)
            party.RegenerateFully();
            party.UnBoost();
            party.UnAttackBoost();
            if (battleWon)
            {
                progress++;
                EnterStoryMode();
            }
            else
                EnterInitializeBattle1Mode();
        }

        private void EnterMemberMenuMode()
        {
            GameMode = GameModes.MemberMenuMode;
            portraitBox.Size = new Vector2(160, 160);
            memberMenuDisplayStatsBox.Size = new Vector2(418, 208);
            // 2024.12.02 introduced constants
            memberMenuCharacterSelectionBox.Size = new Vector2(portraitBox.Size.X + memberMenuDisplayStatsBox.Size.X, (Game1.PREFERREDBACKBUFFERHEIGHT - 184) / 2);
            portraitBox.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - memberMenuDisplayStatsBox.Size.X - portraitBox.Size.X) / 2, (Game1.PREFERREDBACKBUFFERHEIGHT - memberMenuDisplayStatsBox.Size.Y - memberMenuCharacterSelectionBox.Size.Y) / 2);
            memberMenuDisplayStatsBox.Position = new Vector2(portraitBox.Position.X + portraitBox.Size.X, portraitBox.Position.Y);
            memberMenuCharacterSelectionBox.Position = new Vector2(portraitBox.Position.X, (Game1.PREFERREDBACKBUFFERHEIGHT - memberMenuDisplayStatsBox.Size.Y - memberMenuCharacterSelectionBox.Size.Y) / 2 + memberMenuDisplayStatsBox.Size.Y);
        }

        private void EnterStoryMode()
        {
            GameMode = GameModes.StoryMode;
            map = null;
            switch (progress)
            {
                case 0:
                    story = new Story(@"Content\\new_story01.txt");
                    UpdateState_StoryMode();
                    break;

                case 1:
                    story = new Story(@"Content\\new_story02.txt");
                    party.Join(archer);
                    UpdateState_StoryMode();
                    break;

                case 2:
                    story = new Story(@"Content\\new_story03.txt");
                    UpdateState_StoryMode();
                    break;

                case 3:
                    story = new Story(@"Content\\new_story04.txt");
                    party.Join(knight);
                    knight.Level = 2;
                    UpdateState_StoryMode();
                    break;

                case 4:
                    story = new Story(@"Content\\new_story05.txt");
                    UpdateState_StoryMode();
                    break;

                case 5:
                    story = new Story(@"Content\\new_story06.txt");
                    party.Join(mage);
                    mage.Level = 4;
                    UpdateState_StoryMode();
                    break;

                case 6:
                    story = new Story(@"Content\\new_story07.txt");
                    UpdateState_StoryMode();
                    break;

                case 7:
                    story = new Story(@"Content\\new_story08.txt");
                    UpdateState_StoryMode();
                    break;

                case 8:
                    story = new Story(@"Content\\new_story09.txt");
                    UpdateState_StoryMode();
                    break;

                case 9:
                    story = new Story(@"Content\\new_story10.txt");
                    UpdateState_StoryMode();
                    break;

                case 10:
                    story = new Story(@"Content\\new_story11.txt");
                    UpdateState_StoryMode();
                    break;

                default:
                    GameMode = GameModes.TitleScreenMode;
                    break;
            }
        }

        private int LoadNextStoryLine(Story story)
        {
            story_character = story.GetNextCharacter();
            string line = story.GetNextLine();
            int counter = 0;
            int pointer = 0;
            int tempPtr;
            if (line == null)
                return 0;
            while (pointer < line.Length)
            {
                tempPtr = pointer + TEXTBOX_WIDTH;
                if (tempPtr >= line.Length)
                    tempPtr = line.Length - 1;
                else
                    while (tempPtr > pointer && (char) line[tempPtr] != '\n' && (char) line[tempPtr] != '\r' && (char) line[tempPtr] != ' ')
                        tempPtr--;
                textMessage[counter] = line.Substring(pointer, tempPtr - pointer + 1);
                pointer = tempPtr + 1;
                counter++;
            }
            return counter;
        }

        private void LeavePlayerMoveMode()
        {
            mapPositionPlayerMoveMode[party.MemberOnTurn] = map.Position;
        }

        private void LeaveBattleMenuMode()
        {
            GameMode = GameModes.BattleMenuMoveOutMode;
            // 2024.12.02 introduced constants
            tempMenuPosition = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 180) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 120);
            battleMenu.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 180) / 2, Game1.PREFERREDBACKBUFFERHEIGHT);
        }

        private void LeaveBattleMenuMoveOutMode()
        {
            party.Members[party.MemberOnTurn].Visible = true;
            float currentPosition = party.Members[party.MemberOnTurn].Position;
            party.Members[party.MemberOnTurn].Position = backUpPosition;
            map.CalcViable(party.Members[party.MemberOnTurn].Position, party.Members[party.MemberOnTurn].MovePoints, party.Members[party.MemberOnTurn].Flying, enemies);
            party.Members[party.MemberOnTurn].Position = currentPosition;
            GameMode = GameModes.PlayerMoveMode;
        }

        private void LeaveGeneralMenuMode()
        {
            GameMode = GameModes.GeneralMenuMoveOutMode;
            // 2024.12.02 introduced constants
            tempMenuPosition = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 180) / 2, Game1.PREFERREDBACKBUFFERHEIGHT - 120);
            generalMenu.Position = new Vector2((Game1.PREFERREDBACKBUFFERWIDTH - 180) / 2, Game1.PREFERREDBACKBUFFERHEIGHT);
        }

        private void UseItem()
        {
            Item selectedItem = GetSelectedItem();

            if (selectedItem.MagicSpell != null)
                switch (selectedItem.MagicSpell.Type)
                {
                    case Spell.Types.Attack:
                        if (map.AnyCharacterLocatedInMarkedFields(enemies))
                            EnterAttackMenuMode();
                        else
                        {
                            textMessage[0] = "No opponent there.";
                            textMessageLength = 1;
                            returnGameMode = GameModes.ItemMagicSelectionMenuMode;
                            waitForKeyPress = true;
                            moveOut = true;
                            EnterDisplayTextMessageMode();
                        }
                        break;

                    case Spell.Types.Heal:
                    case Spell.Types.Other:
                        if (map.AnyCharacterLocatedInMarkedFields(party))
                            EnterHealMenuMode();
                        else
                        {
                            textMessage[0] = "No party member there.";
                            textMessageLength = 1;
                            returnGameMode = GameModes.ItemMagicSelectionMenuMode;
                            waitForKeyPress = true;
                            moveOut = true;
                            EnterDisplayTextMessageMode();
                        }
                        break;
                }
            else
                switch (selectedItem.Name1 + selectedItem.Name2)
                {
                    // *** add here the items and their effects

                    default:
                        textMessage[0] = "It has no effect.";
                        textMessageLength = 1;
                        returnGameMode = GameModes.ItemMagicSelectionMenuMode;
                        waitForKeyPress = true;
                        moveOut = true;
                        EnterDisplayTextMessageMode();
                        break;
                }
        }

        private void DropItem()
        {
            party.Members[party.MemberOnTurn].Unequip(selectedItemNumber);

            for (int i = selectedItemNumber + 1; i < Character.MAXNUMBER_ITEMS; i++)
            {
                party.Members[party.MemberOnTurn].Items[i - 1] = party.Members[party.MemberOnTurn].Items[i];
                party.Members[party.MemberOnTurn].ItemEquipped[i - 1] = party.Members[party.MemberOnTurn].ItemEquipped[i];
            }

            party.Members[party.MemberOnTurn].Items[3] = null;
            party.Members[party.MemberOnTurn].ItemEquipped[3] = false;

            if (party.Members[party.MemberOnTurn].Items[0] == null)
                party.Members[party.MemberOnTurn].Items = null;
        }

        private void GiveItem()
        {
            if (party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].Items == null)
            {
                party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].Items = new Item[4];
                party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].Items[0] = GetSelectedItem();
                party.Members[party.MemberOnTurn].Unequip(selectedItemNumber);
                DropItem();
                party.Members[party.MemberOnTurn].LookDown();
                selectionBar.Position = map.CalcPosition(party.Members[party.MemberOnTurn].Position);
                EnterEndTurnOfCharacterAndNextCharacterMode();
            }
            else if (party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].Items[3] == null)
            {
                int i;
                for (i = 0; i < 4; i++)
                    if (party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].Items[i] == null)
                        break;
                party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].Items[i] = GetSelectedItem();
                party.Members[party.MemberOnTurn].Unequip(selectedItemNumber);
                DropItem();
                party.Members[party.MemberOnTurn].LookDown();
                selectionBar.Position = map.CalcPosition(party.Members[party.MemberOnTurn].Position);
                EnterEndTurnOfCharacterAndNextCharacterMode();
            }
            else
                EnterSwapItemMoveInMode();
        }

        private void SwapItems()
        {
            Item tempItem = GetSelectedSwapItem();

            party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].Unequip(selectedSwapItemNumber);
            party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].Items[selectedSwapItemNumber] = GetSelectedItem();
            party.Members[party.MemberOnTurn].Unequip(selectedItemNumber);
            party.Members[party.MemberOnTurn].Items[selectedItemNumber] = tempItem;
            party.Members[party.MemberOnTurn].LookDown();
            selectionBar.Position = map.CalcPosition(party.Members[party.MemberOnTurn].Position);
            EnterEndTurnOfCharacterAndNextCharacterMode();
        }

        private Item GetSelectedItem()
        {
            return party.Members[party.MemberOnTurn].Items[selectedItemNumber];
        }

        private Item GetSelectedSwapItem()
        {
            return party.Members[map.GetNextCharacterNumberLocatedInMarkedFields(party, skip)].Items[selectedSwapItemNumber];
        }

        private void ReturnToReturnGameMode()
        {
            switch (returnGameMode)
            {
                case GameModes.BattleMenuMode:
                    EnterBattleMenuMode();
                    break;

                case GameModes.EgressMode:
                    EnterEgressMode();
                    break;

                case GameModes.InitializeBattle2Mode:
                    EnterInitializeBattle2Mode();
                    break;

                case GameModes.EndOfBigBattleMode:
                    EnterEndOfBigBattleMode();
                    break;

                case GameModes.BattleLost2Mode:
                    EnterBattleLost2Mode();
                    break;

                default:
                    GameMode = returnGameMode;
                    break;
            }
        }

        private void setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(Party party1)
        {
            if (party1 == party)
                targetsAttackedOrHealed[numTargetsAttackedOrHealed].BelongsToSide = CharacterPointer.Sides.Player;
            else if (party1 == enemies)
                targetsAttackedOrHealed[numTargetsAttackedOrHealed].BelongsToSide = CharacterPointer.Sides.CPU_Opponents;
        }

        private void MagicAttack(int center, Party defenders)
        {
            switch (selectedMagicSpell.Area[selectedMagicLevel - 1])
            {
                case 1:
                    break;

                case 2:
                    if (map.GetCharacterNumberLocatedInGivenPosition(defenders, center - (int)map.Size.X) != -1)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(defenders, center - (int)map.Size.X);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(defenders);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(defenders, center - 1) != -1)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(defenders, center - 1);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(defenders);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(defenders, center + 1) != -1)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(defenders, center + 1);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(defenders);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(defenders, center + (int)map.Size.X) != -1)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(defenders, center + (int)map.Size.X);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(defenders);
                        numTargetsAttackedOrHealed++;
                    }
                    break;

                case 3:
                    if (map.GetCharacterNumberLocatedInGivenPosition(defenders, center - (int)map.Size.X * 2) != -1)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(defenders, center - (int)map.Size.X * 2);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(defenders);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(defenders, center - (int)map.Size.X - 1) != -1)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(defenders, center - (int)map.Size.X - 1);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(defenders);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(defenders, center - (int)map.Size.X + 1) != -1)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(defenders, center - (int)map.Size.X + 1);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(defenders);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(defenders, center - 2) != -1)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(defenders, center - 2);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(defenders);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(defenders, center + 2) != -1)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(defenders, center + 2);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(defenders);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(defenders, center + (int)map.Size.X - 1) != -1)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(defenders, center + (int)map.Size.X - 1);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(defenders);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(defenders, center + (int)map.Size.X + 1) != -1)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(defenders, center + (int)map.Size.X + 1);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(defenders);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(defenders, center + (int)map.Size.X * 2) != -1)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(defenders, center + (int)map.Size.X * 2);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(defenders);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(defenders, center - (int)map.Size.X) != -1)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(defenders, center - (int)map.Size.X);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(defenders);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(defenders, center - 1) != -1)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(defenders, center - 1);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(defenders);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(defenders, center + 1) != -1)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(defenders, center + 1);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(defenders);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(defenders, center + (int)map.Size.X) != -1)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(defenders, center + (int)map.Size.X);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(defenders);
                        numTargetsAttackedOrHealed++;
                    }
                    break;
            }
        }

        private void MagicHeal(int center, Party party1)
        {
            if (selectedMagicSpell.Name == "AURA" && selectedMagicLevel == 4)
                Aura4(party);
            else
                MagicAttack(center, party1);
        }

        private void Boost(int center, Party party1)
        {
            switch (selectedMagicSpell.Area[selectedMagicLevel - 1])
            {
                case 1:
                    break;

                case 2:
                    if (map.GetCharacterNumberLocatedInGivenPosition(party1, center - (int)map.Size.X) != -1
                        && party1.Members[map.GetCharacterNumberLocatedInGivenPosition(party1, center - (int)map.Size.X)].AgilityBoostedBy == 0)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(party1, center - (int)map.Size.X);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(party1);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(party1, center - 1) != -1
                        && party1.Members[map.GetCharacterNumberLocatedInGivenPosition(party1, center - 1)].AgilityBoostedBy == 0)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(party1, center - 1);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(party1);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(party1, center + 1) != -1
                        && party1.Members[map.GetCharacterNumberLocatedInGivenPosition(party1, center + 1)].AgilityBoostedBy == 0)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(party1, center + 1);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(party1);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(party1, center + (int)map.Size.X) != -1
                        && party1.Members[map.GetCharacterNumberLocatedInGivenPosition(party1, center + (int)map.Size.X)].AgilityBoostedBy == 0)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(party1, center + (int)map.Size.X);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(party1);
                        numTargetsAttackedOrHealed++;
                    }
                    break;

                case 3:
                    if (map.GetCharacterNumberLocatedInGivenPosition(party1, center - (int)map.Size.X * 2) != -1
                        && party1.Members[map.GetCharacterNumberLocatedInGivenPosition(party1, center - (int)map.Size.X) * 2].AgilityBoostedBy == 0)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(party1, center - (int)map.Size.X * 2);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(party1);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(party1, center - (int)map.Size.X - 1) != -1
                        && party1.Members[map.GetCharacterNumberLocatedInGivenPosition(party1, center - (int)map.Size.X - 1)].AgilityBoostedBy == 0)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(party1, center - (int)map.Size.X - 1);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(party1);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(party1, center - (int)map.Size.X + 1) != -1
                        && party1.Members[map.GetCharacterNumberLocatedInGivenPosition(party1, center + (int)map.Size.X) + 1].AgilityBoostedBy == 0)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(party1, center - (int)map.Size.X + 1);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(party1);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(party1, center - 2) != -1
                        && party1.Members[map.GetCharacterNumberLocatedInGivenPosition(party1, center - 2)].AgilityBoostedBy == 0)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(party1, center - 2);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(party1);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(party1, center + 2) != -1
                        && party1.Members[map.GetCharacterNumberLocatedInGivenPosition(party1, center + 2)].AgilityBoostedBy == 0)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(party1, center + 2);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(party1);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(party1, center + (int)map.Size.X - 1) != -1
                        && party1.Members[map.GetCharacterNumberLocatedInGivenPosition(party1, center + (int)map.Size.X)].AgilityBoostedBy == 0)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(party1, center + (int)map.Size.X - 1);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(party1);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(party1, center + (int)map.Size.X + 1) != -1
                        && party1.Members[map.GetCharacterNumberLocatedInGivenPosition(party1, center + (int)map.Size.X + 1)].AgilityBoostedBy == 0)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(party1, center + (int)map.Size.X + 1);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(party1);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(party1, center + (int)map.Size.X * 2) != -1
                        && party1.Members[map.GetCharacterNumberLocatedInGivenPosition(party1, center + (int)map.Size.X * 2)].AgilityBoostedBy == 0)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(party1, center + (int)map.Size.X * 2);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(party1);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(party1, center - (int)map.Size.X) != -1
                        && party1.Members[map.GetCharacterNumberLocatedInGivenPosition(party1, center - (int)map.Size.X)].AgilityBoostedBy == 0)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(party1, center - (int)map.Size.X);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(party1);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(party1, center - 1) != -1
                        && party1.Members[map.GetCharacterNumberLocatedInGivenPosition(party1, center - 1)].AgilityBoostedBy == 0)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(party1, center - 1);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(party1);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(party1, center + 1) != -1
                        && party1.Members[map.GetCharacterNumberLocatedInGivenPosition(party1, center + 1)].AgilityBoostedBy == 0)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(party1, center + 1);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(party1);
                        numTargetsAttackedOrHealed++;
                    }
                    if (map.GetCharacterNumberLocatedInGivenPosition(party1, center + (int)map.Size.X) != -1
                        && party1.Members[map.GetCharacterNumberLocatedInGivenPosition(party1, center + (int)map.Size.X)].AgilityBoostedBy == 0)
                    {
                        targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = map.GetCharacterNumberLocatedInGivenPosition(party1, center + (int)map.Size.X);
                        setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(party1);
                        numTargetsAttackedOrHealed++;
                    }
                    break;
            }
        }

        private void Aura4(Party party1)
        {
            numTargetsAttackedOrHealed = 0;
            for (int i = 0; i < party1.NumPartyMembers; i++)
            {
                if (party1.Members[i].Alive)
                {
                    targetsAttackedOrHealed[numTargetsAttackedOrHealed].WhichOne = i;
                    setTargetsAttackedOrHealedNumTargetsAttackedOrHealedParty(party1);
                    numTargetsAttackedOrHealed++;
                }
            }
        }
    }
}
