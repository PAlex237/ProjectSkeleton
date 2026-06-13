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
        var startX = 100;         // Marginea din stânga de unde încep cărțile
        int dealerHandY = 100;    // Poziția Dealerului (Sus)
        int playerHandY = 550;    // Poziția Jucătorului (Jos)
        
        // === SETUP MÂINI (PENTRU TESTARE VIZUALĂ) ===
        var playerHand = new List<Card>
        {
            new Card(Suit.Hearts, CardValue.Ten),
            new Card(Suit.Spades, CardValue.Seven),
            new Card(Suit.Clubs, CardValue.Four)
        };

        var dealerHand = new List<Card>
        {
            new Card(Suit.Diamonds, CardValue.Ace),
            new Card(Suit.Hearts, CardValue.King)
        };
        
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
                        Console.WriteLine($"Key down: {(KeyCode)ev.Key.Keysym.Scancode}");
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
                DrawHand(r, sdl, dealerHand, dealerHandY);

                // 2. Desenăm mâna Jucătorului
                DrawHand(r, sdl, playerHand, playerHandY);

                sdl.RenderPresent(r);
            }

            ++framesRenderedCounter;
        }

        // === FUNCȚIA DE DESENARE A CĂRȚILOR (ACUM CENTRATĂ DINAMIC) ===
    unsafe void DrawHand(Renderer* r, Sdl sdl, List<Card> hand, int startY)
    {
        if (hand.Count == 0) return; // Dacă nu sunt cărți, nu desenăm nimic

        int cardWidth = 80;
        int cardHeight = 120;
        int spacing = 15;
        int windowWidth = 800; // Lățimea ferestrei tale setată la sdl.CreateWindow

        // 1. Calculăm cât spațiu ocupă toată mâna pe orizontală
        // (Numărul de cărți * Lățimea) + (Numărul de spații libere * Dimensiunea spațiului)
        int totalWidth = (hand.Count * cardWidth) + ((hand.Count - 1) * spacing);

        // 2. Calculăm de unde trebuie să înceapă prima carte pentru a centra întregul grup
        int startX = (windowWidth - totalWidth) / 2;

        for (int i = 0; i < hand.Count; i++)
        {
            var cardRect = new Silk.NET.Maths.Rectangle<int>(startX + i * (cardWidth + spacing), startY, cardWidth, cardHeight);
            
            // Desenăm fața cărții (Alb)
            sdl.SetRenderDrawColor(r, 255, 255, 255, 255);
            sdl.RenderFillRect(r, ref cardRect);
            
            // Desenăm conturul (Negru)
            sdl.SetRenderDrawColor(r, 0, 0, 0, 255);
            sdl.RenderDrawRect(r, ref cardRect);
        }
    }

        unsafe
        {
            sdl.DestroyWindow((Window*)window);
        }

        sdl.Quit();
    }
}