# UI Setup Guide for Endless Runner

This comprehensive guide will help you create a professional-looking UI system for your endless runner game, with detailed steps for each UI element.

## Canvas and Event System Setup

1. Create a Canvas in your scene:
   - GameObject > UI > Canvas
   - Set Canvas Scaler (Script) properties:
     - UI Scale Mode: "Scale With Screen Size"
     - Reference Resolution: 1920 x 1080
     - Match Width or Height: 0.5 (balance between width and height)
   - Set Canvas component properties:
     - Render Mode: "Screen Space - Overlay"
     - Pixel Perfect: Checked
     - Sort Order: 0

2. Add an Event System:
   - GameObject > UI > Event System
   - Keep default settings

3. Create UI Panels parent:
   - Create an empty GameObject named "UI_Panels"
   - Make it a child of the Canvas
   - All UI panels will be children of this object for organization

## Main Game UI (HUD) Panel

1. Create a Panel named "HUD_Panel":
   - GameObject > UI > Panel
   - Make it a child of UI_Panels
   - Set Image component:
     - Source Image: None
     - Color: Clear (Alpha = 0)
   - Set RectTransform to full screen (Anchors: stretch-stretch)

2. Create Score Display:
   - GameObject > UI > Text - TextMeshPro
   - Name it "ScoreText"
   - Position at the top center:
     - Anchors: top-center
     - Pivot: (0.5, 1)
     - Position: (0, -50, 0)
   - TextMeshPro component:
     - Text: "SCORE: 0"
     - Font Size: 42
     - Font Style: Bold
     - Color: White or bright color that stands out
     - Alignment: Center
     - Enable Outline or Shadow for better readability

3. Create Lives Display:
   - Create an empty GameObject named "LivesContainer"
   - Position at top-left corner:
     - Anchors: top-left
     - Pivot: (0, 1)
     - Position: (50, -50, 0)
   - Add Horizontal Layout Group:
     - Spacing: 10
     - Child Alignment: Left
     - Control Child Size: Width and Height
     - Use Child Scale: Checked
   - Add 3 hearts/life icons as children:
     - GameObject > UI > Image
     - Name them "Life_1", "Life_2", "Life_3"
     - Set Source Image to a heart sprite
     - Set Native Size
     - These will be referenced in the UIManager's "lifeIcons" array

4. Create Power-Up Indicators:
   - Create an empty GameObject named "PowerUpIndicators"
   - Position at top-right corner:
     - Anchors: top-right
     - Pivot: (1, 1)
     - Position: (-50, -50, 0)
   - Add Horizontal Layout Group:
     - Spacing: 15
     - Child Alignment: Right
     
   - Add Invincibility Indicator:
     - Create GameObject > UI > Image
     - Name it "InvincibilityIcon"
     - Set sprite to shield icon
     - Add a Slider below it:
       - Set direction: Bottom to Top
       - Set Fill Rect color to match the icon theme
       - Set Min/Max values: 0-1
     - Set inactive by default

   - Add Speed Boost Indicator:
     - Create GameObject > UI > Image
     - Name it "SpeedBoostIcon"
     - Set sprite to lightning/speed icon
     - Add a Slider below it similar to invincibility
     - Set inactive by default

   - Add Extra Life Indicator:
     - Create GameObject > UI > Image
     - Name it "ExtraLifeIcon"
     - Set sprite to heart/plus icon
     - Set inactive by default

## Game Over Panel

1. Create a Panel named "GameOver_Panel":
   - GameObject > UI > Panel
   - Make it a child of UI_Panels
   - Set Image component:
     - Color: Black with Alpha around 0.8
   - Set RectTransform to full screen (Anchors: stretch-stretch)

2. Create Game Over Header:
   - GameObject > UI > Text - TextMeshPro
   - Name it "GameOverHeader"
   - Position at the top portion:
     - Anchors: top-center
     - Pivot: (0.5, 1)
     - Position: (0, -150, 0)
   - TextMeshPro component:
     - Text: "GAME OVER"
     - Font Size: 72
     - Font Style: Bold
     - Color: Red or other attention-grabbing color
     - Alignment: Center
     - Enable Outline or Shadow

3. Create Final Score:
   - GameObject > UI > Text - TextMeshPro
   - Name it "FinalScoreText"
   - Position below header:
     - Anchors: top-center
     - Pivot: (0.5, 1)
     - Position: (0, -250, 0)
   - TextMeshPro component:
     - Text: "Final Score: 0"
     - Font Size: 48
     - Alignment: Center

4. Create Buttons Container:
   - Create an empty GameObject named "GameOver_Buttons"
   - Position at the lower center:
     - Anchors: center
     - Pivot: (0.5, 0.5)
     - Position: (0, -100, 0)
   - Add Vertical Layout Group:
     - Spacing: 20
     - Child Alignment: Center

   - Add Retry Button:
     - GameObject > UI > Button - TextMeshPro
     - Name it "RetryButton"
     - Set Button properties:
       - Normal Color: Dark blue/green
       - Highlighted Color: Slightly lighter
       - Pressed Color: Darker shade
     - Set Text properties:
       - Text: "TRY AGAIN"
       - Font Size: 36
       - Font Style: Bold
       - Color: White

   - Add Main Menu Button:
     - GameObject > UI > Button - TextMeshPro
     - Name it "MainMenuButton"
     - Set Button properties similar to Retry Button
     - Set Text properties:
       - Text: "MAIN MENU"
       - Font Size: 36
       - Font Style: Bold
       - Color: White

5. Set this panel to inactive by default

## Victory Panel

1. Create a Panel named "Victory_Panel":
   - GameObject > UI > Panel
   - Make it a child of UI_Panels
   - Set Image component:
     - Color: Blue or Green with Alpha around 0.8
   - Set RectTransform to full screen (Anchors: stretch-stretch)

2. Create Victory Header:
   - GameObject > UI > Text - TextMeshPro
   - Name it "VictoryHeader"
   - Position at the top portion:
     - Anchors: top-center
     - Pivot: (0.5, 1)
     - Position: (0, -150, 0)
   - TextMeshPro component:
     - Text: "LEVEL COMPLETE!"
     - Font Size: 72
     - Font Style: Bold
     - Color: Gold or bright yellow
     - Alignment: Center
     - Add animations or particle effects for celebration

3. Create Rewards Container:
   - Create an empty GameObject named "RewardsContainer"
   - Position below header:
     - Anchors: top-center
     - Pivot: (0.5, 1)
     - Position: (0, -250, 0)
   - Add Vertical Layout Group:
     - Spacing: 15
     - Child Alignment: Center

   - Add Coins Awarded Text:
     - GameObject > UI > Text - TextMeshPro
     - Name it "CoinsAwardedText"
     - TextMeshPro component:
       - Text: "Coins: +0"
       - Font Size: 36
       - Alignment: Center

   - Add Page Awarded Text (if using):
     - GameObject > UI > Text - TextMeshPro
     - Name it "PageAwardedText"
     - TextMeshPro component:
       - Text: "New Page Unlocked!"
       - Font Size: 36
       - Color: Gold
       - Alignment: Center

4. Create Continue Button:
   - GameObject > UI > Button - TextMeshPro
   - Name it "ContinueButton"
   - Position at the bottom center:
     - Anchors: bottom-center
     - Pivot: (0.5, 0)
     - Position: (0, 150, 0)
   - Set Button properties:
     - Normal Color: Green
     - Highlighted Color: Slightly lighter green
     - Pressed Color: Darker green
   - Set Text properties:
     - Text: "CONTINUE"
     - Font Size: 42
     - Font Style: Bold
     - Color: White

5. Set this panel to inactive by default

## Pause Panel

1. Create a Panel named "Pause_Panel":
   - GameObject > UI > Panel
   - Make it a child of UI_Panels
   - Set Image component:
     - Color: Black with Alpha around 0.7
   - Set RectTransform to full screen (Anchors: stretch-stretch)

2. Create Pause Header:
   - GameObject > UI > Text - TextMeshPro
   - Name it "PauseHeader"
   - Position at the top portion:
     - Anchors: top-center
     - Pivot: (0.5, 1)
     - Position: (0, -150, 0)
   - TextMeshPro component:
     - Text: "PAUSED"
     - Font Size: 64
     - Font Style: Bold
     - Color: White
     - Alignment: Center

3. Create Buttons Container:
   - Create an empty GameObject named "PauseButtons"
   - Position at the center:
     - Anchors: center
     - Pivot: (0.5, 0.5)
   - Add Vertical Layout Group:
     - Spacing: 20
     - Child Alignment: Center

   - Add Resume Button:
     - GameObject > UI > Button - TextMeshPro
     - Name it "ResumeButton"
     - Set Button properties:
       - Normal Color: Blue
       - Highlighted Color: Slightly lighter blue
       - Pressed Color: Darker blue
     - Set Text properties:
       - Text: "RESUME"
       - Font Size: 42
       - Font Style: Bold
       - Color: White

   - Add Main Menu Button:
     - GameObject > UI > Button - TextMeshPro
     - Name it "PauseMainMenuButton"
     - Set Button properties similar to Resume Button but different color
     - Set Text properties:
       - Text: "MAIN MENU"
       - Font Size: 42
       - Font Style: Bold
       - Color: White

4. Set this panel to inactive by default

## UI Manager Configuration

1. Create an empty GameObject named "UIManager" in your scene hierarchy
2. Add the UIManager script to it
3. Configure panel references in the Inspector:
   ```
   [Header("UI Panels")]
   [SerializeField] private GameObject mainGameUI;     // Assign HUD_Panel
   [SerializeField] private GameObject gameOverUI;     // Assign GameOver_Panel
   [SerializeField] private GameObject winUI;          // Assign Victory_Panel
   [SerializeField] private GameObject pauseMenuUI;    // Assign Pause_Panel
   ```

4. Configure HUD elements:
   ```
   [Header("In-Game UI")]
   [SerializeField] private TextMeshProUGUI scoreText;           // Assign ScoreText
   [SerializeField] private GameObject[] lifeIcons;              // Assign Life_1, Life_2, Life_3
   [SerializeField] private GameObject invincibilityIcon;        // Assign InvincibilityIcon
   [SerializeField] private GameObject speedBoostIcon;           // Assign SpeedBoostIcon
   [SerializeField] private GameObject extraLifeIcon;            // Assign ExtraLifeIcon
   [SerializeField] private Slider invincibilitySlider;          // Assign InvincibilityIcon's Slider
   [SerializeField] private Slider speedBoostSlider;             // Assign SpeedBoostIcon's Slider
   ```

5. Configure Game Over UI:
   ```
   [Header("Game Over UI")]
   [SerializeField] private TextMeshProUGUI finalScoreText;      // Assign FinalScoreText
   [SerializeField] private Button retryButton;                  // Assign RetryButton
   [SerializeField] private Button gameOverReturnButton;         // Assign MainMenuButton
   ```

6. Configure Victory UI:
   ```
   [Header("Win UI")]
   [SerializeField] private TextMeshProUGUI coinsAwardedText;    // Assign CoinsAwardedText
   [SerializeField] private TextMeshProUGUI pageAwardedText;     // Assign PageAwardedText
   [SerializeField] private Button continueButton;               // Assign ContinueButton
   ```

## Button Events Configuration

1. Set up the Retry Button:
   - Click the "+" in the On Click() event in the Inspector
   - Drag the UIManager GameObject to the object field
   - Select UIManager > RestartGame function from dropdown

2. Set up the Game Over Return Button:
   - Click the "+" in the On Click() event
   - Drag the UIManager GameObject to the object field
   - Select UIManager > ReturnToMainMenu function

3. Set up the Continue Button:
   - Click the "+" in the On Click() event
   - Drag the UIManager GameObject to the object field
   - Select UIManager > ContinueToNextLevel function

4. Configure PauseManager:
   - Create GameObject named "PauseManager" if not exists
   - Add the PauseManager script
   - Assign references:
     - UI Manager: drag the UIManager GameObject
     - Resume Button: drag the ResumeButton
     - Main Menu Button: drag the PauseMainMenuButton

## Testing and Finalization

1. Enter Play mode and test UI transitions:
   - Verify all panels start in the correct state (only HUD visible)
   - Test pausing/unpausing with Escape key
   - Simulate game over and victory conditions

2. Polish UI with animations (optional):
   - Add Animator components to panels for fade-in/out effects
   - Add UI animation scripts for scores counting up
   - Add particle effects for victory celebrations

3. Accessibility considerations:
   - Ensure text has sufficient contrast with backgrounds
   - Make buttons large enough for easy tapping on mobile
   - Consider adding optional sound effects for UI interactions 