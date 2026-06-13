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
    // === ÎNCĂRCAREA IMAGINII CARDS.BMP ===
    IntPtr cardTexture = IntPtr.Zero;
    unsafe
    {
        // 1. Ocolim Silk.NET și cerem funcțiile direct din fișierul nativ SDL2.dll
        var context = new SdlContext();
        IntPtr rwFromFilePtr = context.GetProcAddress("SDL_RWFromFile");
        IntPtr loadBmpRwPtr = context.GetProcAddress("SDL_LoadBMP_RW");

        if (rwFromFilePtr != IntPtr.Zero && loadBmpRwPtr != IntPtr.Zero)
        {
            // 2. Definim semnăturile funcțiilor C folosind pointeri (delegate* unmanaged)
            var RWFromFile = (delegate* unmanaged[Cdecl]<byte*, byte*, IntPtr>)rwFromFilePtr;
            var LoadBMP_RW = (delegate* unmanaged[Cdecl]<IntPtr, int, Surface*>)loadBmpRwPtr;

            // 3. Pregătim textele în format C (ASCII + un caracter Null '\0' la final)
            byte[] fileBytes = System.Text.Encoding.ASCII.GetBytes("Assets/cards.bmp\0");
            byte[] modeBytes = System.Text.Encoding.ASCII.GetBytes("rb\0");

            fixed (byte* pFile = fileBytes)
            fixed (byte* pMode = modeBytes)
            {
                // 4. Apelăm funcțiile C originale!
                IntPtr rwOps = RWFromFile(pFile, pMode);
                if (rwOps != IntPtr.Zero)
                {
                    Surface* surface = LoadBMP_RW(rwOps, 1); // 1 = închide fișierul după citire
                    if (surface != null)
                    {
                        cardTexture = (IntPtr)sdl.CreateTextureFromSurface((Renderer*)renderer, surface);
                        sdl.FreeSurface(surface);
                        Console.WriteLine("Imaginea a fost încărcată cu succes prin metoda directă!");
                    }
                    else
                    {
                        Console.WriteLine("Eroare la decodarea imaginii BMP din memorie.");
                    }
                }
                else
                {
                    Console.WriteLine("Eroare la citirea fișierului cards.bmp de pe disc.");
                }
            }
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
        
        bool quit = false;
       
        while (!quit)
        {
            while (sdl.PollEvent(ref ev) != 0)
            {
                if (ev.Type == (uint)EventType.Quit)
                {
                    quit = true;
                    break;
                }

                switch (ev.Type)
                {
                    // Am păstrat doar Windowevent, restul au fost colapsate pentru claritate (poți lăsa gol ce nu folosești)
                    case (uint)EventType.Windowevent:
                        break;
                    case (uint)EventType.Keydown:
                    {
                        var keyCode = (KeyCode)ev.Key.Keysym.Scancode;

                        if (keyCode == KeyCode.H) // Dacă jucătorul apasă 'H'
                        {
                            playerHand.Add(deck.Draw()); // Tragem o carte reală din pachet
                            Console.WriteLine($"Jucătorul a dat Hit! Ai acum {playerHand.Count} cărți.");
                        }
                        else if (keyCode == KeyCode.S) // Dacă jucătorul apasă 'S'
                        {
                            Console.WriteLine("Jucătorul a dat Stand! Tura se termină.");
                            // Aici vom adăuga logica prin care Dealerul trage cărți mai târziu
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

                // 1. Desenăm mâna Dealerului
                DrawHand(r, sdl, dealerHand, dealerHandY, cardTexture);

                // 2. Desenăm mâna Jucătorului
                DrawHand(r, sdl, playerHand, playerHandY, cardTexture);

                sdl.RenderPresent(r);
            }

            ++framesRenderedCounter;
        }

        // === FUNCȚIA DE DESENARE CU IMAGINI ===
        unsafe void DrawHand(Renderer* r, Sdl sdl, List<Card> hand, int startY, IntPtr texture)
        {
            if (hand.Count == 0 || texture == IntPtr.Zero) return;

            // Dimensiunile de pe IMAGINE (Sursa)
            int spriteCardWidth = 167; // 2171 / 13
            int spriteCardHeight = 220; // 880 / 4 (Schimbă în 176 dacă ai 5 rânduri pe imagine)

            // Dimensiunile pe ECRAN (Destinația - cât de mari vrei să apară în joc)
            int destWidth = 80;
            int destHeight = 120;
            int spacing = 15;
            int windowWidth = 800;

            // Centrarea pe ecran
            int totalWidth = (hand.Count * destWidth) + ((hand.Count - 1) * spacing);
            int startX = (windowWidth - totalWidth) / 2;

            for (int i = 0; i < hand.Count; i++)
            {
                Card card = hand[i];

                // 1. Dreptunghiul SURSĂ (Decupăm cartea corectă din cards.bmp)
                var srcRect = new Silk.NET.Maths.Rectangle<int>(
                    card.GetSpriteColumn() * spriteCardWidth, // Axa X (Coloana)
                    card.GetSpriteRow() * spriteCardHeight,   // Axa Y (Rândul)
                    spriteCardWidth,
                    spriteCardHeight
                );

                // 2. Dreptunghiul DESTINAȚIE (Unde și cât de mare o punem pe ecran)
                var destRect = new Silk.NET.Maths.Rectangle<int>(
                    startX + i * (destWidth + spacing), 
                    startY, 
                    destWidth, 
                    destHeight
                );

                // 3. Copiem bucata de textură pe ecran!
                sdl.RenderCopy(r, (Texture*)texture, ref srcRect, ref destRect);
            }
        }
        unsafe
        {
            sdl.DestroyWindow((Window*)window);
        }

        sdl.Quit();
    }
}