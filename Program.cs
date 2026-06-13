using System.Collections.Generic;
using System.Diagnostics;
using Silk.NET.SDL;

namespace TheAdventure;

public static class Program
{
    public static void Main()
    {
        var sdl = new Sdl(new SdlContext());

        UInt64 framesRenderedCounter = 0;
        var timer = new Stopwatch();

        ReadOnlySpan<byte> keyboardState;
        unsafe
        {
            keyboardState = new(sdl.GetKeyboardState(null), (int)KeyCode.Count);
        }

        Span<byte> mouseButtonStates = stackalloc byte[(int)MouseButton.Count];

        var ev = new Event();

        var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitAudio | Sdl.InitEvents | Sdl.InitTimer | Sdl.InitGamecontroller |
                                     Sdl.InitJoystick);
        if (sdlInitResult < 0)
        {
            throw new InvalidOperationException("Failed to initialize SDL.");
        }

        IntPtr window;
        unsafe
        {
            window = (IntPtr)sdl.CreateWindow(
                "The Adventure - Blackjack", Sdl.WindowposUndefined, Sdl.WindowposUndefined, 800, 800,
                (uint)WindowFlags.Resizable | (uint)WindowFlags.AllowHighdpi
            );

            if (window == IntPtr.Zero)
            {
                var ex = sdl.GetErrorAsException();
                if (ex != null)
                {
                    throw ex;
                }

                throw new Exception("Failed to create window.");
            }
        }

        IntPtr renderer;
        unsafe
        {
            renderer = (IntPtr)sdl.CreateRenderer((Window*)window, -1, (uint)RendererFlags.Accelerated);
            sdl.RenderSetVSync((Renderer*)renderer, 1);
        }

        if (renderer == IntPtr.Zero)
        {
            var ex = sdl.GetErrorAsException();
            if (ex != null)
            {
                throw ex;
            }

            throw new Exception("Failed to create renderer.");
        }

        // === COORDONATELE MESEI DE JOC ===
        int dealerHandY = 100;    // Poziția Dealerului (Sus)
        int playerHandY = 550;    // Poziția Jucătorului (Jos)

    // === ÎNCĂRCAREA IMAGINILOR (CARDS.BMP ȘI BACK.BMP) ===
    IntPtr cardTexture = IntPtr.Zero;
    IntPtr backTexture = IntPtr.Zero;
    IntPtr fontTexture = IntPtr.Zero;
    unsafe
    {
        var context = new SdlContext();
        IntPtr rwFromFilePtr = context.GetProcAddress("SDL_RWFromFile");
        IntPtr loadBmpRwPtr = context.GetProcAddress("SDL_LoadBMP_RW");

        if (rwFromFilePtr != IntPtr.Zero && loadBmpRwPtr != IntPtr.Zero)
        {
            // 1. Definim funcțiile C o singură dată pentru tot blocul
            var RWFromFile = (delegate* unmanaged[Cdecl]<byte*, byte*, IntPtr>)rwFromFilePtr;
            var LoadBMP_RW = (delegate* unmanaged[Cdecl]<IntPtr, int, Surface*>)loadBmpRwPtr;

            // 2. Modul de citire ("rb") e la fel pentru ambele
            byte[] modeBytes = System.Text.Encoding.ASCII.GetBytes("rb\0");
            
            fixed (byte* pMode = modeBytes)
            {
                // --- ÎNCĂRCARE CARDS.BMP ---
                byte[] cardsFileBytes = System.Text.Encoding.ASCII.GetBytes("Assets//cards.bmp\0");
                fixed (byte* pCardsFile = cardsFileBytes)
                {
                    IntPtr rwOpsCards = RWFromFile(pCardsFile, pMode);
                    if (rwOpsCards != IntPtr.Zero)
                    {
                        Surface* surfaceCards = LoadBMP_RW(rwOpsCards, 1);
                        if (surfaceCards != null)
                        {
                            cardTexture = (IntPtr)sdl.CreateTextureFromSurface((Renderer*)renderer, surfaceCards);
                            sdl.FreeSurface(surfaceCards);
                            Console.WriteLine("cards.bmp încărcat cu succes!");
                        }
                    }
                }

                // --- ÎNCĂRCARE BACK.BMP ---
                byte[] backFileBytes = System.Text.Encoding.ASCII.GetBytes("Assets//back.bmp\0");
                fixed (byte* pBackFile = backFileBytes)
                {
                    IntPtr rwOpsBack = RWFromFile(pBackFile, pMode);
                    if (rwOpsBack != IntPtr.Zero)
                    {
                        Surface* surfaceBack = LoadBMP_RW(rwOpsBack, 1);
                        if (surfaceBack != null)
                        {
                            backTexture = (IntPtr)sdl.CreateTextureFromSurface((Renderer*)renderer, surfaceBack);
                            sdl.FreeSurface(surfaceBack);
                            Console.WriteLine("back.bmp încărcat cu succes!");
                        }
                    }
                }
                // --- ÎNCĂRCARE FONT.BMP ---
                byte[] fontFileBytes = System.Text.Encoding.ASCII.GetBytes("Assets/font.bmp\0");
                fixed (byte* pFontFile = fontFileBytes)
                {
                    IntPtr rwOpsFont = RWFromFile(pFontFile, pMode);
                    if (rwOpsFont != IntPtr.Zero)
                    {
                        Surface* surfaceFont = LoadBMP_RW(rwOpsFont, 1);
                        if (surfaceFont != null)
                        {   // Setăm negrul (0, 0, 0) ca fiind transparent
                            unsafe 
                            {
                                sdl.SetColorKey(surfaceFont, 1, sdl.MapRGB(surfaceFont->Format, 0, 0, 0));
                            }
                            fontTexture = (IntPtr)sdl.CreateTextureFromSurface((Renderer*)renderer, surfaceFont);
                            sdl.FreeSurface(surfaceFont);
                            Console.WriteLine("font.bmp a fost încărcat cu succes!");
                        }
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("Eroare critică: Nu am putut găsi funcțiile native în SDL2.dll!");
        }
    }
        // === SETUP PACHET ȘI MÂINI ===

        // 1. Generăm cele 52 de cărți standard
        var initialCards = new List<Card>();
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (CardValue value in Enum.GetValues(typeof(CardValue)))
            {
                initialCards.Add(new Card(suit, value));
            }
        }

        // 2. Inițializăm pachetul dându-i lista de cărți nou creată
        var deck = new Deck<Card>(initialCards);
        deck.Shuffle(); // Amestecăm pachetul

        var playerHand = new List<Card>();
        var dealerHand = new List<Card>();

        // Împărțim cărțile de început (2 pentru Jucător, 2 pentru Dealer)
        playerHand.Add(deck.Draw());
        playerHand.Add(deck.Draw());

        dealerHand.Add(deck.Draw());
        dealerHand.Add(deck.Draw());
        int playerBudget = 0;
        int currentBet = 50;
        bool isBettingPhase = true;
        bool isPlayerTurn = true;
        bool isGameOver = false;
        bool quit = false;
        playerHand.Clear();
        dealerHand.Clear();
        string savePath = "Assets//save.txt";
        if(File.Exists(savePath))
        {
            if(int.TryParse(File.ReadAllText(savePath), out int loadedBudget))
            {
                playerBudget = loadedBudget;
            }
        }
        static string GetScoreDisplay(List<Card> hand)
        {
            int currentScore = GetTotalValue(hand);
            
            // Calculăm varianta în care toate Asurile ar fi 1
            int hardScore = 0;
            int acesCount = 0;
            foreach (var card in hand)
            {
                if (card.Value == CardValue.Jack || card.Value == CardValue.Queen || card.Value == CardValue.King)
                {
                    hardScore += 10;
                }
                else if (card.Value == CardValue.Ace)
                {
                    acesCount++;
                    hardScore += 1;
                }
                else
                {
                    hardScore += (int)card.Value;
                }
            }

            // Dacă avem Asuri și scorul calculat cu As=11 este diferit de cel cu As=1 
            // și nu am depășit 21, înseamnă că avem o mână flexibilă (soft)
            if (acesCount > 0 && currentScore != hardScore && currentScore <= 21)
            {
                return $"{hardScore}/{currentScore}";
            }

            return currentScore.ToString();
        }
        static int GetTotalValue(List<Card> hand)
        {
            int totalValue = 0;
            int acesCount = 0;

            foreach (var card in hand)
            {
                if (card.Value == CardValue.Jack || card.Value == CardValue.Queen || card.Value == CardValue.King)
                {
                    totalValue += 10;
                }
                else if (card.Value == CardValue.Ace)
                {
                    totalValue += 11;
                    acesCount++;
                }
                else
                {
                    totalValue += (int)card.Value;
                }
            }

            while (totalValue > 21 && acesCount > 0)
            {
                totalValue -= 10;
                acesCount--;
            }

            return totalValue;
        }
        while (!quit)
        {
            while (sdl.PollEvent(ref ev) != 0)
            {
                if (ev.Type == (uint)EventType.Quit)
                {
                    quit = true;
                    File.WriteAllText(savePath, playerBudget.ToString());
                    break;
                }

                switch (ev.Type)
                {
                    case (uint)EventType.Windowevent:
                        break;
                    case (uint)EventType.Keydown:
                    {
                        var keyCode = (KeyCode)ev.Key.Keysym.Scancode;

                        // FAZA 1: Înainte de a paria (Ecran de bet)
                        if (isBettingPhase)
                        {
                            // Butoanele + și - (Săgeata Sus / Săgeata Jos)
                            if (keyCode == KeyCode.Up) 
                            { 
                                // Nu te lăsa să pariezi mai mult decât ai în balanță
                                if (currentBet + 10 <= playerBudget) currentBet += 10; 
                            }
                            if (keyCode == KeyCode.Down) 
                            { 
                                // Pariul minim să fie de cel puțin 10
                                if (currentBet - 10 >= 10) currentBet -= 10; 
                            }

                            // Începe runda
                            if (keyCode == KeyCode.R )
                            {
                                if (playerBudget >= currentBet)
                                {   
                                    if(deck.Count < 10) // Dacă pachetul are mai puțin de 10 cărți, reinițializează-l
                                    {
                                        deck = new Deck<Card>(initialCards);
                                        deck.Shuffle();
                                        Console.WriteLine("Pachetul a fost reinițializat și amestecat!");
                                    }
                                    playerHand.Clear();
                                    dealerHand.Clear();

                                    playerHand.Add(deck.Draw());
                                    playerHand.Add(deck.Draw());

                                    dealerHand.Add(deck.Draw());
                                    dealerHand.Add(deck.Draw());

                                    isPlayerTurn = true;
                                    isGameOver = false;
                                    isBettingPhase = false; // Ascunde + și -, începe tura
                                    Console.WriteLine($"--- RUNDĂ NOUĂ --- Ai pariat {currentBet}$.");
                                }
                                else
                                {
                                    Console.WriteLine("Fonduri insuficiente!");
                                }
                            }
                        }
                        // FAZA 2: În timpul jocului (Hit sau Stand)
                        else if (isPlayerTurn && !isGameOver)
                        {
                            if (keyCode == KeyCode.H)
                            {
                                playerHand.Add(deck.Draw());
                                int currentScore = GetTotalValue(playerHand);
                                
                                Console.WriteLine($"Ai tras o carte! Scor curent: {currentScore}");

                                if (currentScore > 21)
                                {
                                    isPlayerTurn = false;
                                    isGameOver = true;
                                    
                                    Console.WriteLine($"BUST! Ai depășit 21. AI PIERDUT {currentBet}$!");
                                    playerBudget -= currentBet; // Scădem banii direct
                                    isBettingPhase = true;      // Înapoi la faza de pariere
                                }
                            }
                            else if (keyCode == KeyCode.S)
                            {
                                isPlayerTurn = false;
                                isGameOver = true;
                                Console.WriteLine("Ai dat Stand! Acum e rândul Dealerului.");

                                while (GetTotalValue(dealerHand) < 17)
                                {
                                    dealerHand.Add(deck.Draw());
                                    Console.WriteLine("Dealerul trage o carte...");
                                }

                                int playerTotal = GetTotalValue(playerHand);
                                int dealerTotal = GetTotalValue(dealerHand);

                                Console.WriteLine($"--- SCOR FINAL --- Tu: {playerTotal} | Dealer: {dealerTotal}");

                                if (dealerTotal > 21)
                                {
                                    Console.WriteLine($"Dealerul a depășit 21! AI CÂȘTIGAT {currentBet}$!");
                                    playerBudget += currentBet;
                                }
                                else if (dealerTotal > playerTotal)
                                {
                                    Console.WriteLine($"Dealerul are un scor mai mare. AI PIERDUT {currentBet}$!");
                                    playerBudget -= currentBet;
                                }
                                else if (dealerTotal < playerTotal)
                                {
                                    Console.WriteLine($"Ai un scor mai mare! AI CÂȘTIGAT {currentBet}$!");
                                    playerBudget += currentBet;
                                }
                                else
                                {
                                    Console.WriteLine("Egalitate (Push) - Balanța rămâne neschimbată.");
                                }

                                Console.WriteLine($"Buget total actual: {playerBudget}$");
                                isBettingPhase = true; // Permite un nou pariu
                            }
                        }
                        
                        break;
                    }
                }
            }

            var elapsed = timer.Elapsed;
            timer.Restart();

            // === BLOCUL DE DESENARE ===
            unsafe
            {
                var r = (Renderer*)renderer;
                sdl.SetRenderDrawColor(r, 0, 100, 0, 255); // Masa de joc (Verde)
                sdl.RenderClear(r);
                // Afișăm permanent balanța și pariul curent sus
                DrawText(r, sdl, fontTexture, $"BALANTA: {playerBudget}$", 50, 700);
                DrawText(r, sdl, fontTexture, $"PARIU: {currentBet}$", 50, 740);
                // 1. Desenăm mâna Dealerului
                
                DrawHand(r, sdl, dealerHand, dealerHandY, cardTexture, backTexture, isDealerHand: isPlayerTurn);
                
                // 2. Desenăm mâna Jucătorului
                DrawHand(r, sdl, playerHand, playerHandY, cardTexture, backTexture);
                

                
                // === AFIȘARE SCOR JUCĂTOR ===
                int playerTotal = GetTotalValue(playerHand);
                if(playerTotal !=0)
                {
                    DrawText(r, sdl, fontTexture, $"SCORUL TAU: {GetScoreDisplay(playerHand)}", 50, playerHandY - 40);
                }
                // === AFIȘARE SCOR DEALER (Doar când nu mai e rândul tău) ===
                if (!isPlayerTurn)
                {
                    int dealerTotal = GetTotalValue(dealerHand);
                    if(dealerTotal !=0)
                    DrawText(r, sdl, fontTexture, $"SCOR DEALER: {GetScoreDisplay(dealerHand)}", 50, dealerHandY - 40);

                    // Putem afișa un mesaj de status în centrul ecranului
                    if (playerTotal > 21) DrawText(r, sdl, fontTexture, "BUST!", 350, 300);
                    else if (dealerTotal > 21) DrawText(r, sdl, fontTexture, "DEALER BUST! AI CASTIGAT!", 200, 300);
                    else if (dealerTotal > playerTotal) DrawText(r, sdl, fontTexture, "AI PIERDUT!", 320, 300);
                    else if (dealerTotal < playerTotal) DrawText(r, sdl, fontTexture, "AI CASTIGAT!", 320, 300);
                    else DrawText(r, sdl, fontTexture, "EGALITATE!", 330, 300);
                }

                sdl.RenderPresent(r);
            }

            ++framesRenderedCounter;
        }
        // === FUNCȚIA DE DESENARE TEXT ===
        unsafe void DrawText(Renderer* r, Sdl sdl, IntPtr fontTex, string text, int startX, int startY)
        {
            if (fontTex == IntPtr.Zero || string.IsNullOrEmpty(text)) return;

            int charSpriteWidth = 14;  
            int charSpriteHeight = 18; 

            int destCharWidth = 24; 
            int destCharHeight = 24;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                int asciiValue = (int)c;

                int charIndex = asciiValue - 32;

                // Prevenim afișarea caracterelor inexistente în grilă
                if (charIndex < 0 || charIndex >= 18 * 7) charIndex = 0; 

                int col = charIndex % 18;
                int row = charIndex / 18;

                var srcRect = new Silk.NET.Maths.Rectangle<int>(
                    col * charSpriteWidth,
                    row * charSpriteHeight,
                    charSpriteWidth,
                    charSpriteHeight
                );

                var destRect = new Silk.NET.Maths.Rectangle<int>(
                    startX + (i * destCharWidth),
                    startY,
                    destCharWidth,
                    destCharHeight
                );

                sdl.RenderCopy(r, (Texture*)fontTex, ref srcRect, ref destRect);
            }
        }
        // === FUNCȚIA DE DESENARE CU A DOUA CARTE ASCUNSĂ LA DEALER ===
unsafe void DrawHand(Renderer* r, Sdl sdl, List<Card> hand, int startY, IntPtr cardTex, IntPtr backTex, bool isDealerHand = false)
{
    if (hand.Count == 0 || cardTex == IntPtr.Zero) return;

    int spriteCardWidth = 167; 
    int spriteCardHeight = 220;

    int destWidth = 80;
    int destHeight = 120;
    int spacing = 15;
    int windowWidth = 800;

    int totalWidth = (hand.Count * destWidth) + ((hand.Count - 1) * spacing);
    int startX = (windowWidth - totalWidth) / 2;

    for (int i = 0; i < hand.Count; i++)
    {
        Card card = hand[i];
        
        var destRect = new Silk.NET.Maths.Rectangle<int>(
            startX + i * (destWidth + spacing),
            startY,
            destWidth,
            destHeight
        );

        if (isDealerHand && i == 1)
        {
            if (backTex != IntPtr.Zero)
            {
                sdl.RenderCopy(r, (Texture*)backTex, null, &destRect);
            }
        }
        else
        {
            var srcRect = new Silk.NET.Maths.Rectangle<int>(
                card.GetSpriteColumn() * spriteCardWidth,
                card.GetSpriteRow() * spriteCardHeight,
                spriteCardWidth,
                spriteCardHeight
            );

            sdl.RenderCopy(r, (Texture*)cardTex, &srcRect, &destRect);
        }

        // Conturul negru pentru finisaj vizual
        sdl.SetRenderDrawColor(r, 0, 0, 0, 255);
        sdl.RenderDrawRect(r, ref destRect);
    }
}
        unsafe
        {
            sdl.DestroyWindow((Window*)window);
        }

        sdl.Quit();
    }
}