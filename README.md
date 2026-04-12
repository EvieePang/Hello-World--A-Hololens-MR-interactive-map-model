# Educational Earth Model

<img width="1431" height="770" alt="image" src="https://github.com/user-attachments/assets/c384db8f-8bfe-47b6-9d29-a3f7a11227af" />

This repository contains an Educational Earth Model developed in Unity for HoloLens, designed to provide an interactive, multi-layer, and multi-modal way of exploring geographic information.

## Getting Started

### 1. Open the Scene

In Unity, open the following scene:

`Assets → Scenes → Sample Scene`

This is the main entry scene for the Earth Model.

### 2. Enter Play Mode and Initial State

After entering Play Mode, the Earth will appear in its initial state, where countries are not yet selectable.

To activate the country-selection mode:

- Scale up the Earth model until its `Transform → Scale` reaches **0.07** or larger.

Once this threshold is reached, the system switches into interactive country selection mode.

This design helps avoid unintended selections when the Earth is displayed at a very small scale.

### 3. Selecting a Country

Countries can be selected in two different ways:

**a. Hand interaction**  
Use ray interaction followed by a pinch gesture on the target country.

**b. Voice interaction (English)**  
Speak the country name in English to select it directly.

If voice commands do not work:

- Exit Play Mode
- Go to `Edit → Project Settings → XR Plug-in Management`
- Click **Fix All** to refresh the XR configuration
- Re-enter Play Mode — voice interaction should now work correctly

### 4. Map Layer Menu Initialization

After a country is selected, the map layer menu will only appear once your **left hand** is detected within the HoloLens tracking range.

At first appearance, use your left hand to slightly adjust the menu’s initial position.

After initialization, please use the **white handle block** on the menu to reposition it more precisely.

This prevents accidental menu placement and ensures ergonomic interaction.

### 5. Exiting a Country and Exploring Another One

Once you finish exploring a country:

- Click the **EXIT** button on the menu
- Continue clicking **EXIT** until:
  - The system returns to the initial Earth state
  - The menu disappears

You may then select another country and repeat the exploration process.

This step is important to correctly reset the interaction state.

## Demo Video

For a more intuitive understanding of the interaction workflow, please watch our demo video:

[Demo Video Link](https://drive.google.com/file/d/1kXYoKnjLMLGT4QSnS03a-AfypcyluSk9/view?usp=sharing)

The video demonstrates the complete process step by step, including scaling, country selection, voice interaction, and menu manipulation.
