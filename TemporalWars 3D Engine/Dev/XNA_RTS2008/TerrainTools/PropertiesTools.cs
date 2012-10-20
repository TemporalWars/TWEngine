#region File Description
//-----------------------------------------------------------------------------
// PropertiesTools.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using ImageNexus.BenScharbach.TWEngine.Common;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.Enums;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.SteeringBehaviors;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.GameScreens;
using ImageNexus.BenScharbach.TWEngine.InstancedModels;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.SceneItems.Enums;
using ImageNexus.BenScharbach.TWEngine.Terrain;
using ImageNexus.BenScharbach.TWEngine.Terrain.Enums;
using ImageNexus.BenScharbach.TWEngine.Terrain.Structs;
using ImageNexus.BenScharbach.TWEngine.TerrainTools;
using ImageNexus.BenScharbach.TWEngine.TerrainTools.Structs;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using Microsoft.Xna.Framework;
using Color = System.Drawing.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace TWEngine.TerrainTools
{
    ///<summary>
    /// The <see cref="PropertiesTools"/> class is used to update <see cref="SceneItem"/> properties, set <see cref="TerrainWaypoints"/>
    /// and <see cref="TerrainTriggerAreas"/>, as well as update <see cref="SceneItem"/> materials.
    ///</summary>
    public partial class PropertiesTools : Form
    {
        // 7/22/2008

        #region Delegates

        ///<summary>
        /// Set the delegate for the <see cref="PopulateUsingTargetItem"/>.
        ///</summary>
        public delegate void PopulateUsingTargetItem();

        #endregion

        private readonly Game _game;
        private readonly ITerrainShape _terrainShape;
        private SceneItem _currentSceneItem;
        private int _instancedItemPickedIndex; // 5/17/2012 - Saves the Picked Index for ScenaryItems.
        private ListViewItem _itemDnD;
        private Point _mousePoint;

        private Rectangle _rectangle;
        private bool _resettingValues;

        private bool _tbAngleFired;
        private bool _tbHeightFired;
        private bool _tbScaleFired;

        private bool _nupAngelFired;
        private bool _nupHeightFired;
        private bool _nupScaleFired;

        #region Properties

        ///<summary>
        /// Get or set the <see cref="SceneItemWithPick"/> as AbstractBehavior's target item.
        ///</summary>
        public SceneItemWithPick BehaviorsTargetItem { get; set; }

        ///<summary>
        /// Get or set the <see cref="PopulateUsingTargetItem"/> delegate.
        ///</summary>
        public PopulateUsingTargetItem PopulateUsingTargetItemDelegate { get; set; }

        #endregion

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public PropertiesTools(Game game)
        {
            try // 6/22/2010
            {
                InitializeComponent();

                _game = game;

                // 11/20/2009 - Need to turn of FOW, otherwise, blinking will occur.
                var fogOfWar = (IFogOfWar) game.Services.GetService(typeof (IFogOfWar));
                if (fogOfWar != null) fogOfWar.IsVisible = false;


                // Get TerrainShape Interface
                _terrainShape = (ITerrainShape) game.Services.GetService(typeof (ITerrainShape));

                // 3/3/2009 - Set in EditMode for TerrainShape
                TerrainShape.TerrainIsIn = TerrainIsIn.EditMode;

                // 10/13/2009 - Set X-Y-Z Waypoints numericUpDown fields Max.
                nupWayPt_X.Maximum = TerrainData.MapWidthToScale;
                nupWayPt_Z.Maximum = TerrainData.MapHeightToScale;

                // 10/15/2009 - Make sure ListView 'WaypointsPaths' is not MultiSelect.
                lvWaypointsPaths.MultiSelect = false;

                // 2/9/2010 - Create the Procedural Material parameters reference list
                CreateAndPopulateMaterialParamRefs();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PropertiesTools constructor threw the exception " + ex.Message);
#endif
            }
        }

        ///<summary>
        /// Constructor, which call the <see cref="InitializeComponent"/>.
        ///</summary>
        public PropertiesTools()
        {
            InitializeComponent();
        }

        // 5/9/2008; // 2/2/2010: Updated by refactoring common code into the 3 new methods.
        ///<summary>
        /// Needs to be called each time a <see cref="SceneItem"/> is updated in this Tool;
        /// when called, this will update all the Tools form values.
        ///</summary>
        ///<param name="sceneItem"><see cref="SceneItem"/> instance</param>
        ///<exception cref="ArgumentNullException">Thrown when <paramref name="sceneItem"/> instance is null.</exception>
        public void LinkSceneItemToTool(SceneItem sceneItem)
        {
            // 10/6/2009 - Check if null.
            if (sceneItem == null)
                throw new ArgumentNullException("sceneItem",
                                                @"The SceneItem instance given is NULL!  Null SceneItem's can not be used in this method.");

            try // 6/22/2010
            {
                // Set to Resetting Values state
                // This is needed, since the Event Handler will Fire, since I am
                // changing their values below; however, I don't want these changes
                // reflective back to the Behaviors.
                _resettingValues = true;
                ClearValues(); // 10/6/2009 - Clear all values.
                _currentSceneItem = sceneItem; // 10/6/2009 - Set sceneItem

                // 5/17/2012 - Save the current Linked IndexValue for ScenaryItems.
                var scenaryItemScene = sceneItem as ScenaryItemScene;
                if (scenaryItemScene != null)
                {
                    _instancedItemPickedIndex = scenaryItemScene.InstancedItemPickedIndex;
                }

                // 2/2/2010 - Get current SceneItem type atts.
                UpdateAttsFromCurrentSceneItem(sceneItem);
                // 2/2/2010 - Get InstancedItem type atts.
                UpdateAttsFromInstancedItemCast(sceneItem);
                // 2/2/2010 - Get SceneItemWithPick type atts.
                UpdateAttsFromSceneItemWithPickCast(sceneItem);

                // 2/8/2010 - Populate the 'PickByPart' combo box.
                PopulateComboBox_PickByPart();

                // Done Resetting values, so let's turn off.
                _resettingValues = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("LinkSceneItemToTool method threw the exception " + ex.Message);
#endif
            }
        }

        // 6/2/2012
        /// <summary>
        /// Checks if the given tabIndex if currently active.
        /// </summary>
        /// <param name="tabIndex">TabIndex to check</param>
        /// <returns>true/false of result.</returns>
        public bool IsTabIndexActive(int tabIndex)
        {
            return (tcMain.SelectedIndex == tabIndex);
        }

        // 2/1/2010
        /// <summary>
        ///  Updates attributes based on the current SceneItem instance.
        /// </summary>
        /// <param name="sceneItem">SceneItem instance</param>
        private void UpdateAttsFromCurrentSceneItem(SceneItem sceneItem)
        {
            try // 6/22/2010
            {
                // 10/14/2008 - Update SceneItem GroupBox values
                // Get Class name, using Reflections
                Type myTargetType = sceneItem.GetType();
                tbSceneItemClassName.Text = myTargetType.Name;

                // 10/6/2009 - Get user defined 'Name', used for scripting conditions.
                txtUserDefinedName.Text = sceneItem.Name;

                // Update Position
                // 1st - Let's set the Max value spinner control can go to for Pos to be
                //       the Terrain's Max Map Length.
                txtPosX.Maximum = TerrainData.MapWidthToScale;
                txtPosZ.Maximum = TerrainData.MapHeightToScale;
                txtPosX.Value = (decimal) sceneItem.Position.X;
                txtPosZ.Value = (decimal) sceneItem.Position.Z;

                // 10/20/2009 - Update PlayerNumber
                nudPlayerNumber.Value = sceneItem.PlayerNumber;

                // 10/15/2008 - Update MaxForce
                nupMaxForce.Value = (decimal) sceneItem.MaxForce;

                // Update Height
                // 1st - Let's set the Max & Min values for TrackBar to be 500+-
                //       actual Height value.
                float heightAtPos = TerrainData.GetTerrainHeight(sceneItem.Position.X, sceneItem.Position.Z);
                tbHeight.Maximum = (int) (heightAtPos + 500);
                tbHeight.Minimum = (int) (heightAtPos - 500);
                tbHeight.Value = (int) sceneItem.Position.Y;

                // 10/13/2008 - Update NumberUpDown control
                nupHeight.Maximum = tbHeight.Maximum;
                nupHeight.Minimum = tbHeight.Minimum;

                // Update Rotation Y
                Vector3 eulerRotation;
                Quaternion quaternion = sceneItem.Rotation;
                MathUtils.QuaternionToEuler(ref quaternion, out eulerRotation);
                float tmpAngle = eulerRotation.Y;
                tbAngle.Value = (int) tmpAngle;
                // 9/24/2008 - Update NumberUpDown control
                nupAngle.Value = tbAngle.Value;

                // 6/4/2012 - Update Scale
                nupScale.Value = (decimal) sceneItem.Scale.Y;
                tbScale.Value = (int) sceneItem.Scale.Y;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("UpdateAttsFromCurrentSceneItem method threw the exception " + ex.Message);
#endif
            }
        }

        // 2/2/2010
        /// <summary>
        /// Cast current SceneItem to be a SceneItemWithPick, to update
        /// attributes based on this type.
        /// </summary>
        /// <param name="sceneItem">SceneItem instance to cast</param>
// ReSharper disable ParameterTypeCanBeEnumerable.Local
        private void UpdateAttsFromSceneItemWithPickCast(SceneItem sceneItem)
// ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            try // 6/22/2010
            {
                // If Playable SceneItem
                var sceneItemWithPick = (sceneItem as SceneItemWithPick);
                if (sceneItemWithPick == null) return;

                // 8/12/2009
                AStarItem aStarItem = sceneItemWithPick.AStarItemI;

                // Get SceneItemOwner Instance Number
                tbSceneItemNumber.Text = sceneItemWithPick.SceneItemNumber.ToString(CultureInfo.InvariantCulture);

                // Retrieve the SceneItem's Current State Data
                tbItemState.Text = (aStarItem != null) ? aStarItem.ItemState.ToString() : "None";
                tbAIOrderIssued.Text = sceneItemWithPick.AIOrderIssued.ToString();
                tbAttackOn.Text = sceneItemWithPick.AttackOn.ToString(CultureInfo.InvariantCulture);
                tbAttackSceneItem.Text = (sceneItemWithPick.AttackSceneItem != null)
                                             ? ((SceneItemWithPick) sceneItemWithPick.AttackSceneItem).SceneItemNumber.
                                                   ToString(CultureInfo.InvariantCulture)
                                             : "None";
                tbPathToQueueCount.Text = (aStarItem != null)
                                              ? aStarItem.PathToQueue.Count.ToString(CultureInfo.InvariantCulture)
                                              : "Empty";
                tbPathToStackCount.Text = (aStarItem != null)
                                              ? aStarItem.PathToStack.Count.ToString(CultureInfo.InvariantCulture)
                                              : "Empty";
                tbMoveToPosition.Text = (aStarItem != null) ? aStarItem.MoveToPosition.ToString() : "None";
                tbGoalPosition.Text = (aStarItem != null) ? aStarItem.GoalPosition.ToString() : "None";

                // 7/13/2009
                ChangeRequestItem? instanceDataNode;
                InstancedItem.GetInstancedModel_InstanceDataNode(ref sceneItemWithPick.ShapeItem.InstancedItemData,
                                                                 out instanceDataNode);
                tbM44Channel.Text = (instanceDataNode != null)
                                        ? instanceDataNode.Value.Transform.M44.ToString(CultureInfo.InvariantCulture)
                                        : "Null";

                // Update Behaviors, if any.
                UpdateBehaviors();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("UpdateAttsFromSceneItemWithPickCast method threw the exception " + ex.Message);
#endif
            }
        }

        // 2/2/2010
        /// <summary>
        /// Cast current SceneItem to be a <see cref="IInstancedItem"/> interface, to update
        /// attributes based on this type.
        /// </summary>
        /// <param name="sceneItem"><see cref="SceneItem"/> instance</param>
        private void UpdateAttsFromInstancedItemCast(SceneItem sceneItem)
        {
            try // 6/22/2010
            {
                var instancedItem = (sceneItem.ShapeItem as IInstancedItem);
                if (instancedItem == null)
                {
                    tbSceneItemTypeName.Text = @"Not an Instanced Item";
                    return;
                }

                // Get SceneItemOwner Type name
                tbSceneItemTypeName.Text = instancedItem.ItemType.ToString();
                // Get ItemType Number
                tbSceneItemTypeNumber.Text = ((int) instancedItem.ItemType).ToString(CultureInfo.InvariantCulture);
                // Get ItemInstanceKey
                tbItemInstance.Text = instancedItem.ItemInstanceKey.ToString(CultureInfo.InvariantCulture);

                // Update A* PathFinding Data
                cbIsPathBlocked.Checked = instancedItem.IsPathBlocked;
                // 8/1/2008 - Make sure Checked is true before trying to assign value
                if (cbIsPathBlocked.Checked)
                    txtPathSize.Value = instancedItem.PathBlockSize;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("UpdateAttsFromInstancedItemCast method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/6/2009 - Clear all values
        private void ClearValues()
        {
            try // 6/22/2010
            {
                tbSceneItemClassName.Text = string.Empty;
                tbSceneItemTypeName.Text = string.Empty;
                tbSceneItemTypeNumber.Text = string.Empty;
                tbItemInstance.Text = string.Empty;
                tbSceneItemNumber.Text = string.Empty;
                nudPlayerNumber.Value = 0; // 10/20/2009
                txtPosX.Maximum = 0;
                txtPosZ.Maximum = 0;
                txtPosX.Value = 0;
                txtPosZ.Value = 0;
                //nupMaxForce.Value = 0;
                tbHeight.Maximum = 0;
                tbHeight.Minimum = 0;
                tbHeight.Value = 0;
                tbAngle.Value = 0;
                nupAngle.Value = tbAngle.Value;
                cbIsPathBlocked.Checked = false;
                //txtPathSize.Value = 0;
                tbItemState.Text = string.Empty;
                tbAIOrderIssued.Text = string.Empty;
                tbAttackOn.Text = string.Empty;
                tbAttackSceneItem.Text = string.Empty;
                tbPathToQueueCount.Text = string.Empty;
                tbPathToStackCount.Text = string.Empty;
                tbMoveToPosition.Text = string.Empty;
                tbGoalPosition.Text = string.Empty;
                tbM44Channel.Text = string.Empty;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ClearValues method threw the exception " + ex.Message);
#endif
            }
        }

        // 7/6/2009
        private void UpdateBehaviors()
        {
            try // 6/22/2010
            {
                // Set to Resetting Values state
                // This is needed, since the Event Handler will Fire, since I am
                // changing their values below; however, I don't want these changes
                // reflective back to the Behaviors.
                _resettingValues = true;

                var sceneItemWithPick = (_currentSceneItem as SceneItemWithPick); // 8/12/2009
                if (sceneItemWithPick != null)
                {
                    ForceBehaviorsCalculator steering = sceneItemWithPick.ForceBehaviors;

                    // Null Values for TargetItem & Delegate Callback
                    BehaviorsTargetItem = null;
                    PopulateUsingTargetItemDelegate = null;

                    // 12/18/2009 - Resets all the behaviors settings to zero on the tabs.
                    ResetAllBehaviorTabsToZero();

                    // Get AbstractBehavior TabControl collection
                    TabControl.TabPageCollection behaviorTabs = tcBehaviors.TabPages;
                    // Reset all Tab's Fore color to Black
                    for (int i = 0; i < behaviorTabs.Count; i++)
                    {
                        behaviorTabs[i].ForeColor = Color.Black;
                    }

                    // Set Behaviors new values in group tabs.
                    UpdateAllBehaviorTabsWithValues(steering);

                    // 12/18/2009 - Set BotHelper flag
                    if (sceneItemWithPick.AStarItemI != null)
                        cbBotHelper.Checked = sceneItemWithPick.AStarItemI.ItemState == ItemStates.BotHelper;
                }

                // Check if all 3 Flock Behaviors are on.
                if (cbSeparationBehavior.Checked && cbAlignmentBehavior.Checked && cbCohesionBehavior.Checked)
                    cbFlockingBehavior.Checked = true;
                if (cbSeparationActive.Checked && cbAlignmentActive.Checked && cbCohesionActive.Checked)
                    cbFlockActive.Checked = true;

                // Done Resetting values, so let's turn off.
                _resettingValues = false;

                // Force Redraw of Tabs
                tcBehaviors.Refresh();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("UpdateBehaviors method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/18/2009
        /// <summary>
        /// Sets all ForceBehaviors controls, in the Group Tabs,
        /// to their proper values. 
        /// </summary>
        /// <param name="forceBehavior">ForceBehavior to update</param>
        private void UpdateAllBehaviorTabsWithValues(ForceBehaviorsCalculator forceBehavior)
        {
            try // 6/22/2010
            {
                // check if null
                if (forceBehavior == null) return;

                // Get AbstractBehavior TabControl collection
                TabControl.TabPageCollection behaviorTabs = tcBehaviors.TabPages;

                // Get sortKeys used in SortedList
                var behaviorSortKeys = new int[forceBehavior.Behaviors.Keys.Count];
                forceBehavior.Behaviors.Keys.CopyTo(behaviorSortKeys, 0);
                // Iterate Behaviors Array using sortKeys.
                int length = behaviorSortKeys.Length; // 12/18/2009
                for (int i = 0; i < length; i++)
                {
                    var behaviorType = (BehaviorsEnum) behaviorSortKeys[i];
                    AbstractBehavior behavior = forceBehavior.Behaviors[(int) behaviorType];
                    if (behavior == null) continue; // 12/18/2009

                    switch (behaviorType)
                    {
                        case BehaviorsEnum.Arrive:
                            // Set Tab to red
                            behaviorTabs["tabArrive"].ForeColor = Color.Red;
                            break;
                        case BehaviorsEnum.Flee:
                            cbFleeBehavior.Checked = true;
                            var fleeBehavior = (behavior as FleeBehavior);
                            if (fleeBehavior != null)
                            {
                                Vector3 tarPos = fleeBehavior.TargetPosition;
                                fleeTarPos_X.Value = (decimal) tarPos.X;
                                fleeTarPos_Y.Value = (decimal) tarPos.Y;
                                fleeTarPos_Z.Value = (decimal) tarPos.Z;
                                fleePanicDistance.Value = (decimal) fleeBehavior.PanicDistance;
                            }
                            // Set Tab to red
                            behaviorTabs["tabFlee"].ForeColor = Color.Red;
                            break;
                        case BehaviorsEnum.FollowPath:
                            cbFollowPathBehavior.Checked = true;
                            // Set Tab to red
                            behaviorTabs["tabFollowPath"].ForeColor = Color.Red;
                            break;
                        case BehaviorsEnum.OffsetPursuit:
                            cbOffsetPursuitBehavior.Checked = true;
                            var pursuitBehavior = (behavior as OffsetPursuitBehavior);
                            if (pursuitBehavior != null)
                            {
                                Vector3 offsetBy = pursuitBehavior.OffsetBy;
                                offsetBy_X.Value = (decimal) offsetBy.X;
                                offsetBy_Y.Value = (decimal) offsetBy.Y;
                                offsetBy_Z.Value = (decimal) offsetBy.Z;
                            }
                            // Set Tab to red
                            behaviorTabs["tabOffsetPursuit"].ForeColor = Color.Red;
                            break;
                        case BehaviorsEnum.ObstacleAvoidance:
                            cbObstacleAvoidance.Checked = true;
                            // Set Tab to red
                            behaviorTabs["tabObstacleAvoidance"].ForeColor = Color.Red;
                            break;
                        case BehaviorsEnum.Seek:
                            cbSeekBehavior.Checked = true;
                            var seekBehavior = (behavior as SeekBehavior);
                            if (seekBehavior != null)
                            {
                                Vector3 tarPos = seekBehavior.TargetPosition;
                                seekTarPos_X.Value = (decimal) tarPos.X;
                                seekTarPos_Y.Value = (decimal) tarPos.Y;
                                seekTarPos_Z.Value = (decimal) tarPos.Z;
                                seekComfortDistance.Value = (decimal) seekBehavior.ComfortDistance;
                            }
                            // Set Tab to red
                            behaviorTabs["tabSeek"].ForeColor = Color.Red;
                            break;
                        case BehaviorsEnum.Separation:
                            cbSeparationBehavior.Checked = true;
                            var separationBehavior = (behavior as SeparationBehavior);
                            if (separationBehavior != null)
                            {
                                nupSeparationWeight.Value = (decimal) separationBehavior.BehaviorWeight;
                                cbSeparationActive.Checked = separationBehavior.UseBehavior;
                            }
                            // Set Tab to red
                            behaviorTabs["tabFlock"].ForeColor = Color.Red;
                            break;
                        case BehaviorsEnum.Alignment:
                            cbAlignmentBehavior.Checked = true;
                            var alignmentBehavior = (behavior as AlignmentBehavior);
                            if (alignmentBehavior != null)
                            {
                                nupAlignmentWeight.Value = (decimal) alignmentBehavior.BehaviorWeight;
                                cbAlignmentActive.Checked = alignmentBehavior.UseBehavior;
                            }
                            // Set Tab to red
                            behaviorTabs["tabFlock"].ForeColor = Color.Red;
                            break;
                        case BehaviorsEnum.Cohesion:
                            cbCohesionBehavior.Checked = true;
                            var cohesionBehavior = (behavior as CohesionBehavior);
                            if (cohesionBehavior != null)
                            {
                                nupCohesionWeight.Value = (decimal) cohesionBehavior.BehaviorWeight;
                                cbCohesionActive.Checked = cohesionBehavior.UseBehavior;
                            }
                            // Set Tab to red
                            behaviorTabs["tabFlock"].ForeColor = Color.Red;
                            break;
                        case BehaviorsEnum.TurnToFace:
                            cbTurnToFaceBehavior.Checked = true;
                            // Set Tab to red
                            behaviorTabs["tabTurnToFace"].ForeColor = Color.Red;
                            break;
                        case BehaviorsEnum.TurnTurret:
                            cbTurnTurretBehavior.Checked = true;
                            // Set Tab to red
                            behaviorTabs["tabTurnTurret"].ForeColor = Color.Red;
                            break;
                        case BehaviorsEnum.UpdateOrientation:
                            cbUpdateOrientationBehavior.Checked = true;
                            // Set Tab to red
                            behaviorTabs["tabUpdateOrientation"].ForeColor = Color.Red;
                            break;
                        case BehaviorsEnum.Wander:
                            cbWanderBehavior.Checked = true;
                            var wanderBehavior = (behavior as WanderBehavior);
                            if (wanderBehavior != null)
                            {
                                wanderDistance.Value = (decimal) wanderBehavior.WanderDistance;
                                wanderJitter.Value = (decimal) wanderBehavior.WanderJitter;
                                wanderRadius.Value = (decimal) wanderBehavior.WanderRadius;
                            }
                            // Set Tab to red
                            behaviorTabs["tabWander"].ForeColor = Color.Red;
                            break;
                    } // End Switch AbstractBehavior
                } // For Loop SortKeys
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("UpdateAllBehaviorTabsWithValues method threw the exception " + ex.Message ??
                                "No Message");
#endif
            }
        }

        // 12/18/2009
        /// <summary>
        /// Sets all ForceBehaviors controls, in the Group Tabs,
        /// to zero.
        /// </summary>
        private void ResetAllBehaviorTabsToZero()
        {
            try // 6/22/2010
            {
                // Flee
                cbFleeBehavior.Checked = false;
                fleeTarPos_X.Value = 0;
                fleeTarPos_Y.Value = 0;
                fleeTarPos_Z.Value = 0;
                fleePanicDistance.Value = 0;
                tbClassName1.Text = string.Empty;
                tbItemTypeName1.Text = string.Empty;
                tbItemInstance1.Text = string.Empty;
                // Seek  12/18/2009
                cbSeekBehavior.Checked = false;
                seekTarPos_X.Value = 0;
                seekTarPos_Y.Value = 0;
                seekTarPos_Z.Value = 0;
                seekComfortDistance.Value = 0;
                tbClassName3.Text = string.Empty;
                tbItemTypeName3.Text = string.Empty;
                tbItemInstance3.Text = string.Empty;
                // FollowPath
                cbFollowPathBehavior.Checked = false;
                // Offset
                cbOffsetPursuitBehavior.Checked = false;
                offsetBy_X.Value = 0;
                offsetBy_Y.Value = 0;
                offsetBy_Z.Value = 0;
                tbClassName2.Text = string.Empty;
                tbItemTypeName2.Text = string.Empty;
                tbItemInstance2.Text = string.Empty;
                // ObstacleAvoidance
                cbObstacleAvoidance.Checked = false;
                // Flock (Separation, Alignment, Cohesion)
                cbFlockingBehavior.Checked = false;
                cbSeparationBehavior.Checked = false;
                cbAlignmentBehavior.Checked = false;
                cbCohesionBehavior.Checked = false;
                cbSeparationActive.Checked = false;
                cbAlignmentActive.Checked = false;
                cbCohesionActive.Checked = false;
                cbFlockActive.Checked = false;
                // Turns / Orientation
                cbTurnToFaceBehavior.Checked = false;
                cbTurnTurretBehavior.Checked = false;
                cbUpdateOrientationBehavior.Checked = false;
                // Wander
                cbWanderBehavior.Checked = false;
                wanderDistance.Value = 0;
                wanderJitter.Value = 0;
                wanderRadius.Value = 0;
                // BotHelper
                cbBotHelper.Checked = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ResetAllBehaviorTabsToZero method threw the exception " + ex.Message);
#endif
            }
        }

        #region Angle Methods

        // Update Angle Changes back to SceneItem
        private void tbAngle_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // 10/6/2009 - fixes issue of items disappearing!
                if (_resettingValues)
                    return;

                if (_currentSceneItem == null)
                    return;

                // If Fired from User changing Value directly, then let's update
                if (!_nupAngelFired)
                {
                    // 6/11/2012 - Make sure to set InstancedItemPickedIndex for ScenaryItems!
                    var scenaryItem = _currentSceneItem as ScenaryItemScene;
                    if (scenaryItem != null)
                    {
                        scenaryItem.InstancedItemPickedIndex = _instancedItemPickedIndex;
                    }

                    // 6/11/2012 - Updated to new Rotation method.
                    _currentSceneItem.SetRotationByValue(RotationAxisEnum.RotationOnY, tbAngle.Value);
                    TerrainQuadTree.UpdateSceneryCulledList = true;

                    // Update NumberUpDown control
                    _tbAngleFired = true;
                    nupAngle.Value = tbAngle.Value;
                }

                _nupAngelFired = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("tbAngle_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 9/24/2008 - Event handler for when the NumericUpDown control Value changes.
        private void nupAngle_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // 10/6/2009 - fixes issue of items disappearing!
                if (_resettingValues)
                    return;

                if (_currentSceneItem == null)
                    return;

                // If Fired from User changing Value directly, then let's update
                if (!_tbAngleFired)
                {
                    // 6/11/2012 - Make sure to set InstancedItemPickedIndex for ScenaryItems!
                    var scenaryItem = _currentSceneItem as ScenaryItemScene;
                    if (scenaryItem != null)
                    {
                        scenaryItem.InstancedItemPickedIndex = _instancedItemPickedIndex;
                    }

                    // 6/11/2012 - Updated to new Rotation method.
                    _currentSceneItem.SetRotationByValue(RotationAxisEnum.RotationOnY, tbAngle.Value);
                    TerrainQuadTree.UpdateSceneryCulledList = true;

                    // Update Tb control
                    _nupAngelFired = true;
                    tbAngle.Value = (int) nupAngle.Value;
                }
               
                _tbAngleFired = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupAngle_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 6/11/2012 - Sets all instances of the current picked sceneItem to the same rotation value.
        private void btnAngleSetAll_Click(object sender, EventArgs e)
        {
            if (_currentSceneItem == null)
                return;

            var scenaryItem = (_currentSceneItem as ScenaryItemScene);
            if (scenaryItem == null)
                return;

            var currentRotationAngle = (int)nupAngle.Value;
            // set to all instances.
            scenaryItem.SetRotationToAllInstances(RotationAxisEnum.RotationOnY, currentRotationAngle);
            TerrainQuadTree.UpdateSceneryCulledList = true;
        }

        #endregion

        #region Scale Methods

        // 6/11/2012 
        private void tbScale_ValueChanged(object sender, EventArgs e)
        {
            try 
            {
                // fixes issue of items disappearing!
                if (_resettingValues)
                    return;

                if (_currentSceneItem == null)
                    return;

                // If Fired from User changing Value directly, then let's update
                if (!_nupScaleFired)
                {
                    // 6/11/2012 - Make sure to set InstancedItemPickedIndex for ScenaryItems!
                    var scenaryItem = _currentSceneItem as ScenaryItemScene;
                    if (scenaryItem != null)
                    {
                        scenaryItem.InstancedItemPickedIndex = _instancedItemPickedIndex;
                    }

                    Vector3 scale = _currentSceneItem.Scale;
                    scale.X = scale.Y = scale.Z = MathHelper.Clamp(tbScale.Value, 0.01f, 100f);
                    _currentSceneItem.Scale = scale;
                    TerrainQuadTree.UpdateSceneryCulledList = true;

                    // Update NumberUpDown control
                    _tbScaleFired = true;
                    nupScale.Value = (decimal)MathHelper.Clamp(tbScale.Value, 1, 100);
                }

                _nupScaleFired = false;

            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("tbScale_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 6/11/2012 - Event handler for when the Nup control value changes
        private void nupScale_ValueChanged(object sender, EventArgs e)
        {
            try 
            {
                // fixes issue of items disappearing!
                if (_resettingValues)
                    return;

                if (_currentSceneItem == null)
                    return;

                // If Fired from User changing Value directly, then let's update
                if (!_tbScaleFired)
                {
                    // 6/11/2012 - Make sure to set InstancedItemPickedIndex for ScenaryItems!
                    var scenaryItem = _currentSceneItem as ScenaryItemScene;
                    if (scenaryItem != null)
                    {
                        scenaryItem.InstancedItemPickedIndex = _instancedItemPickedIndex;
                    }

                    var scale = _currentSceneItem.Scale;
                    var clampedScaleValue = MathHelper.Clamp((float) nupScale.Value, 0.01f, 100f);
                    scale.X = scale.Y = scale.Z = clampedScaleValue;
                    _currentSceneItem.Scale = scale;
                    TerrainQuadTree.UpdateSceneryCulledList = true;

                    // Update Tb control
                    _nupScaleFired = true;
                    tbScale.Value = (int)clampedScaleValue;
                }

                _tbScaleFired = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupScale_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 6/11/2012 - Sets all instances of the current picked sceneItem to the same scale value.
        private void btnScaleSetAll_Click(object sender, EventArgs e)
        {
            if (_currentSceneItem == null)
                return;

            var scenaryItem = (_currentSceneItem as ScenaryItemScene);
            if (scenaryItem == null)
                return;

            var currentScale = (int)nupScale.Value;

            var scale = _currentSceneItem.Scale;
            scale.X = scale.Y = scale.Z = MathHelper.Clamp(currentScale, 0.01f, 100f);
            _currentSceneItem.Scale = scale;

            // set to all instances.
            scenaryItem.SetScaleToAllInstances(scale);
            TerrainQuadTree.UpdateSceneryCulledList = true;
        }

        #endregion

        #region Height Methods

        // Update Height Changes back to SceneItem
        // ReSharper disable InconsistentNaming
        private void tbHeight_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // 10/6/2009 - fixes issue of items disappearing!
                if (_resettingValues)
                    return;

                 if (_currentSceneItem == null)
                    return;

                // If Fired from User changing Value directly, then let's update
                 if (!_nupHeightFired)
                 {
                     // 6/11/2012 - Make sure to set InstancedItemPickedIndex for ScenaryItems!
                     var scenaryItem = _currentSceneItem as ScenaryItemScene;
                     if (scenaryItem != null)
                     {
                         scenaryItem.InstancedItemPickedIndex = _instancedItemPickedIndex;
                     }

                     // 6/11/2012 - Updated: '_newValue' removed.
                     Vector3 position = _currentSceneItem.Position;
                     var clampedHeightValue = MathHelper.Clamp(tbHeight.Value, tbHeight.Value - 50, tbHeight.Value + 50);
                     position.Y = clampedHeightValue;
                     _currentSceneItem.Position = position;
                     TerrainQuadTree.UpdateSceneryCulledList = true;

                     // 10/13/2008 - Update NumberUpDown control
                     _tbHeightFired = true;
                     nupHeight.Maximum = tbHeight.Value + 50;
                     nupHeight.Minimum = tbHeight.Value - 50;
                     nupHeight.Value = (decimal) clampedHeightValue;
                 }

                _nupHeightFired = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("tbHeight_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/13/2008 - Event handler for when then NumericUpDown control value changes.
        private void nupHeight_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // 10/6/2009 - fixes issue of items disappearing!
                if (_resettingValues)
                    return;

                if (_currentSceneItem == null)
                    return;

                // If Fired from User changing Value directly, then let's update
                if (!_tbHeightFired)
                {
                    // 6/11/2012 - Make sure to set InstancedItemPickedIndex for ScenaryItems!
                    var scenaryItem = _currentSceneItem as ScenaryItemScene;
                    if (scenaryItem != null)
                    {
                        scenaryItem.InstancedItemPickedIndex = _instancedItemPickedIndex;
                    }

                    // 6/11/2012 - Updated: '_newValue' removed.
                    Vector3 position = _currentSceneItem.Position;
                    var clampedHeightValue = MathHelper.Clamp(tbHeight.Value, tbHeight.Value - 50, tbHeight.Value + 50);
                    position.Y = clampedHeightValue;
                    _currentSceneItem.Position = position;
                    TerrainQuadTree.UpdateSceneryCulledList = true;

                    _nupHeightFired = true;
                    tbHeight.Value = (int) nupHeight.Value;
                }

                _tbHeightFired = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupHeight_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 6/11/2012 Sets all instances of the current picked sceneItem to the same height value.
        private void btnHeightSetAll_Click(object sender, EventArgs e)
        {
            if (_currentSceneItem == null)
                return;

            var scenaryItem = (_currentSceneItem as ScenaryItemScene);
            if (scenaryItem == null)
                return;

            var currentHeight = (int)nupHeight.Value;

            var position = _currentSceneItem.Position;
            position.Y = currentHeight;
            _currentSceneItem.Position = position;

            // set to all instances.
            scenaryItem.SetHeightToAllInstances(currentHeight);
            TerrainQuadTree.UpdateSceneryCulledList = true;
        }

        #endregion

        // Update Pos X change back to SceneItem
        private void txtPosX_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // 10/6/2009 - fixes issue of items disappearing!
                if (_resettingValues)
                    return;

                // 1st - If 'IsPathBlocked', we then Remove A* Block data at Old Position
                var scenaryItemScene = (_currentSceneItem as ScenaryItemScene); // 8/12/2009
                var sceneItemWithPick = (_currentSceneItem as SceneItemWithPick); // 8/12/2009

                if ((_currentSceneItem.ShapeItem as IInstancedItem).IsPathBlocked)
                {
                    // 10/29/2008
                    if (scenaryItemScene != null)
                    {
                        scenaryItemScene.RemoveCostAtCurrentPosition();

                        // 6/11/2012 - Make sure to set InstancedItemPickedIndex for ScenaryItems!
                        scenaryItemScene.InstancedItemPickedIndex = _instancedItemPickedIndex;
                    }
                    else
                    {
                        if (sceneItemWithPick != null && sceneItemWithPick.AStarItemI != null)
                            AStarItem.RemoveCostAtCurrentPosition(sceneItemWithPick.AStarItemI);
                    }
                }

                // 6/11/2012 - Updated: '_newValue' removed.
                // 2nd - Update new Position values
                Vector3 position = _currentSceneItem.Position;
                position.X = (float) txtPosX.Value;
                _currentSceneItem.Position = position;

                // 3rd - If 'IsPathBlocked', we then Add A* Block data at New Position
                const int tmpInCost = -1;
                var tmpInSize = (int) txtPathSize.Value;
                if ((_currentSceneItem.ShapeItem as IInstancedItem).IsPathBlocked)
                {
                    // 10/29/2008
                    if (scenaryItemScene != null)
                        scenaryItemScene.SetCostAtCurrentPosition(tmpInCost, tmpInSize);
                    else
                    {
                        if (sceneItemWithPick != null && sceneItemWithPick.AStarItemI != null)
                            AStarItem.SetCostAtCurrentPosition(sceneItemWithPick.AStarItemI, tmpInCost, tmpInSize);
                    }
                }

                // DEBUG: Update
                TerrainShape.PopulatePathNodesArray();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("txtPosX_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // Update Pos Z change back to SceneItem
        private void txtPosZ_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // 10/6/2009 - fixes issue of items disappearing!
                if (_resettingValues)
                    return;

                // 1st - If 'IsPathBlocked', we then Remove A* Block data at Old Position
                var scenaryItemScene = (_currentSceneItem as ScenaryItemScene); // 8/12/2009
                var sceneItemWithPick = (_currentSceneItem as SceneItemWithPick);

                if ((_currentSceneItem.ShapeItem as IInstancedItem).IsPathBlocked)
                {
                    // 10/29/2008
                    if (scenaryItemScene != null)
                    {
                        scenaryItemScene.RemoveCostAtCurrentPosition();

                        // 6/11/2012 - Make sure to set InstancedItemPickedIndex for ScenaryItems!
                        scenaryItemScene.InstancedItemPickedIndex = _instancedItemPickedIndex;
                    }
                    else
                    {
                        if (sceneItemWithPick != null && sceneItemWithPick.AStarItemI != null)
                            AStarItem.RemoveCostAtCurrentPosition(sceneItemWithPick.AStarItemI);
                    }
                }

                // 6/11/2012 - Updated: '_newValue' removed.
                Vector3 position = _currentSceneItem.Position;
                position.Z = (float) txtPosZ.Value;
                _currentSceneItem.Position = position;

                // 3rd - If 'IsPathBlocked', we then Add A* Block data at New Position
                const int tmpInCost = -1;
                var tmpInSize = (int) txtPathSize.Value;
                if ((_currentSceneItem.ShapeItem as IInstancedItem).IsPathBlocked)
                {
                    // 10/29/2008
                    if (scenaryItemScene != null)
                        scenaryItemScene.SetCostAtCurrentPosition(tmpInCost, tmpInSize);
                    else
                    {
                        if (sceneItemWithPick != null && sceneItemWithPick.AStarItemI != null)
                            AStarItem.SetCostAtCurrentPosition(sceneItemWithPick.AStarItemI, tmpInCost, tmpInSize);
                    }
                }

                // DEBUG: Update
                TerrainShape.PopulatePathNodesArray();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("txtPosZ_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 5/7/2008

        ///<summary>
        /// Helper function which checks if a <see cref="Point"/> given is within this window form's client rectangle. 
        /// <see cref="Control.MousePosition"/> point is retrieved from this form, which is already in screen cordinates, and
        /// is compared using a rectangle created with this Windows location; finally, the rectangle's 'Contains'
        /// method is called to see if <see cref="Control.MousePosition"/> is inside.
        ///</summary>
        ///<returns>true/false of result</returns>
        public bool IsMouseInControl()
        {
            try // 6/22/2010
            {
                _mousePoint.X = MousePosition.X;
                _mousePoint.Y = MousePosition.Y;

                // set this Form's ClientRectangle            
                _rectangle.X = Location.X + 5;
                _rectangle.Y = Location.Y + 5;
                _rectangle.Width = Width - 5;
                _rectangle.Height = Height - 5;

                bool isIn;
                _rectangle.Contains(ref _mousePoint, out isIn);

                return isIn;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PropertiesTools classes IsMouseInControl method threw the exception " + ex.Message);
#endif
            }
            return false;
        }

        // 5/13/2008
        // Updates the ScenaryItems A* Path Finding Data
        private void btnUpdatePathInfo_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // 1st - Update Values back into SceneItem
                var instancedItem = (_currentSceneItem.ShapeItem as IInstancedItem); // 8/12/2009
                var scenaryItemScene = (_currentSceneItem as ScenaryItemScene); // 8/12/2009
                var sceneItemWithPick = (_currentSceneItem as SceneItemWithPick); // 8/12/2009

                instancedItem.IsPathBlocked = cbIsPathBlocked.Checked;
                instancedItem.PathBlockSize = (int) txtPathSize.Value;

                // 2nd - Update the A* Graph
                // 10/29/2008  
                if (scenaryItemScene != null)
                    scenaryItemScene.RemoveCostAtCurrentPosition();
                else
                {
                    if (sceneItemWithPick != null && sceneItemWithPick.AStarItemI != null)
                        AStarItem.RemoveCostAtCurrentPosition(sceneItemWithPick.AStarItemI);
                }

                const int tmpInCost = -1;
                var tmpInSize = (int) txtPathSize.Value;
                if (instancedItem.IsPathBlocked)
                {
                    // 10/29/2008
                    if (scenaryItemScene != null)
                        scenaryItemScene.SetCostAtCurrentPosition(tmpInCost, tmpInSize);
                    else
                    {
                        if (sceneItemWithPick != null && sceneItemWithPick.AStarItemI != null)
                            AStarItem.SetCostAtCurrentPosition(sceneItemWithPick.AStarItemI, tmpInCost, tmpInSize);
                    }
                }

                // DEBUG: Update
                TerrainShape.PopulatePathNodesArray();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnUpdatePathInfo_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/15/2008
        // Updates the SceneItemOwner's 'MaxForce' value.
        private void nupMaxForce_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // 10/6/2009
                if (_resettingValues)
                    return;

                var numeric = (NumericUpDown) sender;

                _currentSceneItem.MaxForce = (float) numeric.Value;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupMaxForce_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        // Updates the 'TurnToFaceAbstractBehavior' attribute.
        private void cbTurnToFaceBehavior_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                UpdateItemBehavior(cbTurnToFaceBehavior.Checked, BehaviorsEnum.TurnToFace);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbTurnToFaceBehavior_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        // Updates the 'TurnToFaceAbstractBehavior' attribute.
        private void cbTurnTurretBehavior_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                UpdateItemBehavior(cbTurnTurretBehavior.Checked, BehaviorsEnum.TurnTurret);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbTurnTurretBehavior_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        // Updates the 'TurnToFaceAbstractBehavior' attribute.
        private void cbUpdateOrientationBehavior_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                UpdateItemBehavior(cbUpdateOrientationBehavior.Checked, BehaviorsEnum.UpdateOrientation);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbUpdateOrientationBehavior_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        // Updates the 'FleeAbstractBehavior' attribute.
        private void cbFleeBehavior_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                UpdateItemBehavior(cbFleeBehavior.Checked, BehaviorsEnum.Flee);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbFleeBehavior_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // Updates the 'FleeAbstractBehavior's TargetPos Vector.X value
        private void fleeTarPos_X_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var numeric = (NumericUpDown) sender;

                UpdateItemBehaviorAttribute<FleeBehavior>("TargetPosition", "X", BehaviorsEnum.Flee,
                                                          (float) numeric.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("fleeTarPos_X_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // Updates the 'FleeAbstractBehavior's TargetPos Vector.Y value
        private void fleeTarPos_Y_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var numeric = (NumericUpDown) sender;

                UpdateItemBehaviorAttribute<FleeBehavior>("TargetPosition", "Y", BehaviorsEnum.Flee,
                                                          (float) numeric.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("fleeTarPos_Y_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // Updates the 'FleeAbstractBehavior's TargetPos Vector.Z value
        private void fleeTarPos_Z_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var numeric = (NumericUpDown) sender;

                UpdateItemBehaviorAttribute<FleeBehavior>("TargetPosition", "Z", BehaviorsEnum.Flee,
                                                          (float) numeric.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("fleeTarPos_Z_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // Updates the 'FleeAbstractBehavior's PanicDistance double value
        private void fleePanicDistance_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var numeric = (NumericUpDown) sender;

                UpdateItemBehaviorAttribute<FleeBehavior>("PanicDistance", BehaviorsEnum.Flee, (double) numeric.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("fleePanicDistance_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        // Updates the 'SeekAbstractBehavior' attribute
        private void cbSeekBehavior_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                UpdateItemBehavior(cbSeekBehavior.Checked, BehaviorsEnum.Seek);

                // 12/18/2009 - Set the 'SeekToTargetItem' properly, which controls which Update method to use internally.
                UpdateItemBehaviorAttribute<SeekBehavior>("SeekToTargetItem", BehaviorsEnum.Seek, cbSeekBehavior.Checked);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbSeekBehavior_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/18/2009
        // Updates the 'SeekAbstractBehavior's TargetPos Vector.X value
        private void seekTarPos_X_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var numeric = (NumericUpDown) sender;

                UpdateItemBehaviorAttribute<SeekBehavior>("TargetPosition", "X", BehaviorsEnum.Seek,
                                                          (float) numeric.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("seekTarPos_X_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/18/2009
        // Updates the 'SeekAbstractBehavior's TargetPos Vector.Y value
        private void seekTarPos_Y_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var numeric = (NumericUpDown) sender;

                UpdateItemBehaviorAttribute<SeekBehavior>("TargetPosition", "Y", BehaviorsEnum.Seek,
                                                          (float) numeric.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("seekTarPos_Y_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/18/2009
        // Updates the 'SeekAbstractBehavior's TargetPos Vector.Z value
        private void seekTarPos_Z_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var numeric = (NumericUpDown) sender;

                UpdateItemBehaviorAttribute<SeekBehavior>("TargetPosition", "Z", BehaviorsEnum.Seek,
                                                          (float) numeric.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("seekTarPos_Z_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/18/2009
        // Updates the 'SeekAbstractBehavior's ComfortDistance double value
        private void seekComfortDistance_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var numeric = (NumericUpDown) sender;

                UpdateItemBehaviorAttribute<SeekBehavior>("ComfortDistance", BehaviorsEnum.Seek, (double) numeric.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("seekComfortDistance_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }


        // 12/17/2009
        // Turns on all 3 Behaviors (Separation/Alignment/Cohesion) which make up the 'Flocking' AbstractBehavior.
        private void cbFlockingBehavior_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var checkBox = cbFlockingBehavior.Checked;

                // enable/disable 'Active' check boxes
                cbFlockActive.Enabled = checkBox;
                cbSeparationBehavior.Checked = checkBox;
                cbAlignmentBehavior.Checked = checkBox;
                cbCohesionBehavior.Checked = checkBox;

                // update the behaviors
                UpdateItemBehavior(checkBox, BehaviorsEnum.Separation);
                UpdateItemBehavior(checkBox, BehaviorsEnum.Alignment);
                UpdateItemBehavior(checkBox, BehaviorsEnum.Cohesion);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbFlockingBehavior_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        // Updates the 'SeparationAbstractBehavior' attribute.
        private void cbSeparationBehavior_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // enable/disable 'Active' check box
                cbSeparationActive.Enabled = cbSeparationBehavior.Checked;
                UpdateItemBehavior(cbSeparationBehavior.Checked, BehaviorsEnum.Separation);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbSeparationBehavior_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        // Updates the 'AlignmentAbstractBehavior' attribute.
        private void cbAlignmentBehavior_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // enable/disable 'Active' check box
                cbAlignmentActive.Enabled = cbAlignmentBehavior.Checked;
                UpdateItemBehavior(cbAlignmentBehavior.Checked, BehaviorsEnum.Alignment);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbAlignmentBehavior_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        // Updates the 'CohesionAbstractBehavior' attribute.
        private void cbCohesionBehavior_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // enable/disable 'Active' check box
                cbCohesionActive.Enabled = cbCohesionBehavior.Checked;
                UpdateItemBehavior(cbCohesionBehavior.Checked, BehaviorsEnum.Cohesion);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbCohesionBehavior_Click method threw the exception " + ex.Message);
#endif
            }
        }


        // 10/15/2008
        // Updates the 'Sepatation' Weight float value.
        private void nupSeparationWeight_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                var numeric = (NumericUpDown) sender;

                UpdateItemBehaviorAttribute<SeparationBehavior>("BehaviorWeight", BehaviorsEnum.Separation,
                                                                (float) numeric.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupSeparationWeight_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/15/2008
        // Updates the 'Alignment' Weight float value.
        private void nupAlignmentWeight_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                var numeric = (NumericUpDown) sender;

                UpdateItemBehaviorAttribute<AlignmentBehavior>("BehaviorWeight", BehaviorsEnum.Alignment,
                                                               (float) numeric.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupAlignmentWeight_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/15/2008
        // Updates the 'Cohesion' Weight float value.
        private void nupCohesionWeight_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                var numeric = (NumericUpDown) sender;

                UpdateItemBehaviorAttribute<CohesionBehavior>("BehaviorWeight", BehaviorsEnum.Cohesion,
                                                              (float) numeric.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupCohesionWeight_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        // Updates the 'Flock' Active, by setting all 3 behaviors (Separation/Alignment/Cohesion) to Active.
        private void cbFlockActive_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                bool @checked = cbFlockActive.Checked;

                // Update the Active check marks
                cbSeparationActive.Checked = @checked;
                cbAlignmentActive.Checked = @checked;
                cbCohesionActive.Checked = @checked;

                // Update the Behaviors
                UpdateItemBehaviorAttribute<SeparationBehavior>("UseBehavior", BehaviorsEnum.Separation, @checked);
                UpdateItemBehaviorAttribute<AlignmentBehavior>("UseBehavior", BehaviorsEnum.Alignment, @checked);
                UpdateItemBehaviorAttribute<CohesionBehavior>("UseBehavior", BehaviorsEnum.Cohesion, @checked);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbFlockActive_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        // Updates the 'Separation' Active value
        private void cbSeparationActive_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                UpdateItemBehaviorAttribute<SeparationBehavior>("UseBehavior", BehaviorsEnum.Separation,
                                                                cbSeparationActive.Checked);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbSeparationActive_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        // Updates the 'Alignment' Active value
        private void cbAlignmentActive_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                UpdateItemBehaviorAttribute<AlignmentBehavior>("UseBehavior", BehaviorsEnum.Alignment,
                                                               cbAlignmentActive.Checked);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbAlignmentActive_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        // Updates the 'Cohesion' Active value
        private void cbCohesionActive_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                UpdateItemBehaviorAttribute<CohesionBehavior>("UseBehavior", BehaviorsEnum.Cohesion,
                                                              cbCohesionActive.Checked);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbCohesionActive_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        // Updates the 'WanderAbstractBehavior' attribute.
        private void cbWanderBehavior_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                UpdateItemBehavior(cbWanderBehavior.Checked, BehaviorsEnum.Wander);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbWanderBehavior_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/18/2009
        // Updates the 'BotHelper' attribute.
        private void cbBotHelper_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var sceneItemWithPick = (_currentSceneItem as SceneItemWithPick);

                if (sceneItemWithPick == null) return;

                // 12/18/2009 - Some of the behaviors will need the 'ItemState'
                //              set to 'BotHelper'; otherwise, will not be able to move or react
                //              when in 'Resting' mode.
                sceneItemWithPick.AStarItemI.ItemState = cbBotHelper.Checked ? ItemStates.BotHelper : ItemStates.Resting;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbBotHelper_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // Updates the 'WanderAbstractBehavior's Radius float value
        private void wanderRadius_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var numeric = (NumericUpDown) sender;

                UpdateItemBehaviorAttribute<WanderBehavior>("WanderRadius", BehaviorsEnum.Wander, (float) numeric.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("wanderRadius_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // Updates the 'WanderAbstractBehavior's Distance float value
        private void wanderDistance_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var numeric = (NumericUpDown) sender;

                UpdateItemBehaviorAttribute<WanderBehavior>("WanderDistance", BehaviorsEnum.Wander,
                                                            (float) numeric.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("wanderDistance_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // Updates the 'WanderAbstractBehavior's Jitter float value
        private void wanderJitter_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var numeric = (NumericUpDown) sender;

                UpdateItemBehaviorAttribute<WanderBehavior>("WanderJitter", BehaviorsEnum.Wander, (float) numeric.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("wanderJitter_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        // Updates the 'WanderAbstractBehavior' attribute.
        private void cbFollowPathBehavior_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                UpdateItemBehavior(cbFollowPathBehavior.Checked, BehaviorsEnum.FollowPath);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbFollowPathBehavior_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        // Updates the 'OffsetPursuitAbstractBehavior' attribute.
        private void cbOffsetPursuitBehavior_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // FollowPath should not be on, so let's automatically turn off if this is selected.
                if (cbOffsetPursuitBehavior.Checked)
                    cbFollowPathBehavior.Checked = false;

                UpdateItemBehavior(cbOffsetPursuitBehavior.Checked, BehaviorsEnum.OffsetPursuit);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbOffsetPursuitBehavior_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // Updates the 'OffsetBehavior's OffsetBy Vector.X value
        private void offsetBy_X_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var numeric = (NumericUpDown) sender;

                UpdateItemBehaviorAttribute<OffsetPursuitBehavior>("OffsetBy", "X", BehaviorsEnum.OffsetPursuit,
                                                                   (float) numeric.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("offsetBy_X_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // Updates the 'OffsetBehavior's OffsetBy Vector.Y value
        private void offsetBy_Y_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var numeric = (NumericUpDown) sender;

                UpdateItemBehaviorAttribute<OffsetPursuitBehavior>("OffsetBy", "Y", BehaviorsEnum.OffsetPursuit,
                                                                   (float) numeric.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("offsetBy_Y_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // Updates the 'OffsetBehavior's OffsetBy Vector.Z value
        private void offsetBy_Z_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var numeric = (NumericUpDown) sender;

                UpdateItemBehaviorAttribute<OffsetPursuitBehavior>("OffsetBy", "Z", BehaviorsEnum.OffsetPursuit,
                                                                   (float) numeric.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("offsetBy_Z_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        // Updates the 'ObstacleAvoidanceAbstractBehavior' attribute.
        private void cbObstacleAvoidance_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                UpdateItemBehavior(cbObstacleAvoidance.Checked, BehaviorsEnum.ObstacleAvoidance);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbObstacleAvoidance_Click method threw the exception " + ex.Message);
#endif
            }
        }

        /// <summary>
        /// Delegate function used to populate the Flee Behaviors Target values.
        /// </summary>
        private void PopulateFleeTargetValues()
        {
            try // 6/22/2010
            {
                // Save Ref to SceneItemOwner clicked on, into scene SceneItemOwner's 'ForceBehaviors'.
                var sceneItemWithPick = (_currentSceneItem as SceneItemWithPick);
                if (sceneItemWithPick != null) sceneItemWithPick.ForceBehaviors.TargetItem1 = BehaviorsTargetItem;

                if (BehaviorsTargetItem == null) return;

                // Get Class name, using Reflections
                Type myTargetType = BehaviorsTargetItem.GetType();
                tbClassName1.Text = myTargetType.Name;
                // Get SceneItemOwner Type name
                tbItemTypeName1.Text = BehaviorsTargetItem.ShapeItem.ItemType.ToString();
                // Get SceneItemOwner Instance Number
                tbItemInstance1.Text = BehaviorsTargetItem.SceneItemNumber.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PopulateFleeTargetValues method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        /// <summary>
        /// Delegate function used to populate the Seek Behaviors Target values.
        /// </summary>
        private void PopulateSeekTargetValues()
        {
            try // 6/22/2010
            {
                // Save Ref to SceneItemOwner clicked on, into scene SceneItemOwner's 'ForceBehaviors'.
                var sceneItemWithPick = (_currentSceneItem as SceneItemWithPick);
                if (sceneItemWithPick != null) sceneItemWithPick.ForceBehaviors.TargetItem1 = BehaviorsTargetItem;

                if (BehaviorsTargetItem == null) return;

                // Get Class name, using Reflections
                Type myTargetType = BehaviorsTargetItem.GetType();
                tbClassName3.Text = myTargetType.Name;
                // Get SceneItemOwner Type name
                tbItemTypeName3.Text = BehaviorsTargetItem.ShapeItem.ItemType.ToString();
                // Get SceneItemOwner Instance Number
                tbItemInstance3.Text = BehaviorsTargetItem.SceneItemNumber.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PopulateSeekTargetValues method threw the exception " + ex.Message);
#endif
            }
        }

        /// <summary>
        /// Delegate function used to populate the Offset Behaviors Target values.
        /// </summary>
        private void PopulateOffsetTargetValues()
        {
            try // 6/22/2010
            {
                // Save Ref to SceneItemOwner clicked on, into scene SceneItemOwner's 'ForceBehaviors'.
                var sceneItemWithPick = (_currentSceneItem as SceneItemWithPick);
                if (sceneItemWithPick != null) sceneItemWithPick.ForceBehaviors.TargetItem1 = BehaviorsTargetItem;

                // Calculate OffsetBy Vector, using Our Position, and Target Position
                Vector3 offsetBy = _currentSceneItem.Position - BehaviorsTargetItem.Position;
                offsetBy_X.Value = (decimal) offsetBy.X;
                offsetBy_Y.Value = (decimal) offsetBy.Y;
                offsetBy_Z.Value = (decimal) offsetBy.Z;

                if (BehaviorsTargetItem == null) return;

                // Get Class name, using Reflections
                Type myTargetType = BehaviorsTargetItem.GetType();
                tbClassName2.Text = myTargetType.Name;
                // Get SceneItemOwner Type name
                tbItemTypeName2.Text = BehaviorsTargetItem.ShapeItem.ItemType.ToString();
                // Get SceneItemOwner Instance Number
                tbItemInstance2.Text = BehaviorsTargetItem.SceneItemNumber.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PopulateOffsetTargetValues method threw the exception " + ex.Message);
#endif
            }
        }

        /// <summary>
        /// Populate the Flee GroupBox with SceneItem's 'TargetItem' Information.
        /// </summary>
        private void gbFleeTargetItem_Paint(object sender, PaintEventArgs e)
        {
            try // 6/22/2010
            {
                // Retrieve TargetItem
                var sceneItemWithPick = (_currentSceneItem as SceneItemWithPick);
                if (sceneItemWithPick != null)
                    BehaviorsTargetItem = (SceneItemWithPick) sceneItemWithPick.ForceBehaviors.TargetItem1; // 5/20/2012

                // Set Delegate Function callback for timer event.
                PopulateUsingTargetItemDelegate = PopulateFleeTargetValues;

                if (BehaviorsTargetItem == null) return;

                // Get Class name, using Reflections
                Type myTargetType = BehaviorsTargetItem.GetType();
                tbClassName1.Text = myTargetType.Name;

                // Get SceneItemOwner Type name
                tbItemTypeName1.Text = BehaviorsTargetItem.ShapeItem.ItemType.ToString();
                // Get SceneItemOwner Instance Number
                tbItemInstance1.Text = BehaviorsTargetItem.SceneItemNumber.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PopulateOffsetTargetValues method threw the exception " + ex.Message);
#endif
            }
        }

        /// <summary>
        /// Populate the Offset GroupBox with SceneItem's 'TargetItem' Information.
        /// </summary>
        private void gbOffsetTargetItem_Paint(object sender, PaintEventArgs e)
        {
            try // 6/22/2010
            {
                // Retrieve TargetItem
                var sceneItemWithPick = (_currentSceneItem as SceneItemWithPick);
                if (sceneItemWithPick != null)
                    BehaviorsTargetItem = (SceneItemWithPick) sceneItemWithPick.ForceBehaviors.TargetItem1; // 5/20/2012

                // Set Delegate Function callback for timer event.
                PopulateUsingTargetItemDelegate = PopulateOffsetTargetValues;

                if (BehaviorsTargetItem == null) return;

                // Get Class name, using Reflections
                Type myTargetType = BehaviorsTargetItem.GetType();
                tbClassName2.Text = myTargetType.Name;

                // Get SceneItemOwner Type name
                tbItemTypeName2.Text = BehaviorsTargetItem.ShapeItem.ItemType.ToString();
                // Get SceneItemOwner Instance Number
                tbItemInstance2.Text = BehaviorsTargetItem.SceneItemNumber.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("gbOffsetTargetItem_Paint method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        /// <summary>
        /// Populate the Seek GroupBox with SceneItem's 'TargetItem' Information.
        /// </summary>
        private void gbSeekTargetItem_Paint(object sender, PaintEventArgs e)
        {
            try // 6/22/2010
            {
                // Retrieve TargetItem
                var sceneItemWithPick = (_currentSceneItem as SceneItemWithPick);
                if (sceneItemWithPick != null)
                    BehaviorsTargetItem = (SceneItemWithPick) sceneItemWithPick.ForceBehaviors.TargetItem1; // 5/20/2012

                // Set Delegate Function callback for timer event.
                PopulateUsingTargetItemDelegate = PopulateSeekTargetValues;

                if (BehaviorsTargetItem == null) return;

                // Get Class name, using Reflections
                Type myTargetType = BehaviorsTargetItem.GetType();
                tbClassName3.Text = myTargetType.Name;

                // Get SceneItemOwner Type name
                tbItemTypeName3.Text = BehaviorsTargetItem.ShapeItem.ItemType.ToString();
                // Get SceneItemOwner Instance Number
                tbItemInstance3.Text = BehaviorsTargetItem.SceneItemNumber.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("gbSeekTargetItem_Paint method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/13/2008
        /// <summary>
        /// Used to Draw a tab with different appearance.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tcBehaviors_DrawItem(object sender, DrawItemEventArgs e)
        {
            try // 6/22/2010
            {
                Graphics g = e.Graphics;
                Brush textBrush;

                // Get the SceneItemOwner from the collection.
                TabPage tabPage = tcBehaviors.TabPages[e.Index];

                // Check if Tab 'selected'
                if (e.State == DrawItemState.Selected)
                {
                    textBrush = new SolidBrush(tabPage.ForeColor);
                    g.FillRectangle(Brushes.LightBlue, e.Bounds);
                }
                else
                {
                    textBrush = new SolidBrush(tabPage.ForeColor);
                    e.DrawBackground();
                }

                // Use our own font.
                var tabFont = new Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel);

                g.DrawString(tabPage.Text, tabFont, textBrush, e.Bounds.X, e.Bounds.Y);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("tcBehaviors_DrawItem method threw the exception " + ex.Message);
#endif
            }
        }

        // 3/3/2009 - Clears all Red Picks when in edit mode.
        private void btnClearRedPicks_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                InstancedItem.ClearAllPicksFromDictionary();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnClearRedPicks_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 3/3/2009 - Deletes all SceneItems from terrain
        private void btnDeleteAllItems_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var terrainScreen = (TerrainScreen) _game.Services.GetService(typeof (TerrainScreen));

                terrainScreen.EditModeDeleteAllSceneItems();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnDeleteAllItems_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 7/2/2009 - Deletes all Instances of the currently Picked 'ItemType'
        private void btnDeleteAllPickedInstances_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var terrainScreen = (TerrainScreen) _game.Services.GetService(typeof (TerrainScreen));

                terrainScreen.EditModeDeleteSpecificSceneItem(_currentSceneItem.ShapeItem.ItemType);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnDeleteAllPickedInstances_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 4/8/2009 - Sets the current location as 'Loc-1' on map.
        private void btnSetAsLoc1_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // get values for x/y
                var position = new Vector3 {X = (float) txtPosX.Value, Y = 0, Z = (float) txtPosZ.Value};

                // get MapMarkerPosition struct from terrianShape
                MapMarkerPositions mapMarkers = _terrainShape.MapMarkerPositions;

                // Update Location-1
                mapMarkers.markerLoc1 = position;

                // save new MapMarkerPosition data back into TerrainShape
                _terrainShape.MapMarkerPositions = mapMarkers;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnSetAsLoc1_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 4/8/2009 - Sets the current location as 'Loc-2' on map.
        private void btnSetAsLoc2_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // get values for x/y
                var position = new Vector3 {X = (float) txtPosX.Value, Y = 0, Z = (float) txtPosZ.Value};

                // get MapMarkerPosition struct from terrianShape
                MapMarkerPositions mapMarkers = _terrainShape.MapMarkerPositions;

                // Update Location-2
                mapMarkers.markerLoc2 = position;

                // save new MapMarkerPosition data back into TerrainShape
                _terrainShape.MapMarkerPositions = mapMarkers;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnSetAsLoc2_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009
        /// <summary>
        /// EventHandler for when the user updates the check box state.
        /// </summary>
        private void cbDefineArea_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // turn on/off the timerAreas component.
                switch (cbDefineArea.CheckState)
                {
                    case CheckState.Checked:
                        TerrainTriggerAreas.DoDefineArea = true;
                        break;
                    case CheckState.Unchecked:
                    case CheckState.Indeterminate:
                        TerrainTriggerAreas.DoDefineArea = false;
                        break;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbDefineArea_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/17/2009; // 1/15/2010 - Fix.
        /// <summary>
        /// EventHandler for when the user updates the check box state.
        /// </summary>
        private void cbDefineWaypoints_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                switch (cbDefineWaypoints.CheckState)
                {
                    case CheckState.Checked:
                        TerrainWaypoints.DoDefineWaypoint = true;
                        break;
                    case CheckState.Unchecked:
                    case CheckState.Indeterminate:
                        TerrainWaypoints.DoDefineWaypoint = false;
                        break;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbDefineWaypoints_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 9/28/2009
        /// <summary>
        /// Used to add new 'TriggerAreas' defined, to the internal Dictionary
        /// of the 'TerrainTriggerAreas' class.
        /// </summary>
        private void btnAddTriggerArea_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // check if name field is empty.
                if (string.IsNullOrEmpty(txtAreaName.Text))
                {
                    lblTriggerErrorMessages.Text = @"An Area name MUST be given first.";
                    return;
                }

                // check if name already exist in Dictionary
                if (!TerrainTriggerAreas.AddNewTriggerArea(txtAreaName.Text))
                {
                    lblTriggerErrorMessages.Text = @"Name chosen already exists! Please choose a new name.";
                    txtAreaName.Text = string.Empty; // 4/8/2010 - Clear out current name.
                    return;
                }

                // 9/29/2009 - Add to listView
                lvTriggerAreas.Items.Add(txtAreaName.Text);

                // 4/8/2010 - Clear out current name, since added to list now.
                txtAreaName.Text = string.Empty;

                lblTriggerErrorMessages.Text = @"New Trigger Area ('" + txtAreaName.Text + @"') added.";
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnAddTriggerArea_Click method threw the exception " + ex.Message);
#endif
            }
        }


        // 9/29/2009
        /// <summary>
        /// EventHandler to capture the click of the PopulateList button, which will then
        /// populate the list with current 'TriggerAreas'.
        /// </summary>
        private void btnPopulateList_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // make sure records exist, and listView is not Null!
                PopulateTriggerAreasListView();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnPopulateList_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 9/29/2009
        /// <summary>
        /// Helper method, which populates the ListView with the given records from the
        /// TerrainTriggerAreas class.
        /// </summary>
        private void PopulateTriggerAreasListView()
        {
            try // 6/22/2010
            {
                if (TerrainTriggerAreas.TriggerAreas.Count <= 0 || lvTriggerAreas == null) return;

                // iterates dictionary
                lvTriggerAreas.Clear();
                foreach (var triggerArea in TerrainTriggerAreas.TriggerAreas)
                {
                    lvTriggerAreas.Items.Add(triggerArea.Key);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PopulateTriggerAreasListView method threw the exception " + ex.Message);
#endif
            }
        }

        // 9/29/2009
        /// <summary>
        /// Searches the given ListView for the 'TriggerArea' name, and if found, highlights
        /// backColor to Yellow, and marks as 'Selected' item.
        /// </summary>
        /// <param name="triggerName"></param>
        public void SelectTriggerAreaInListView(string triggerName)
        {
            try // 6/22/2010
            {
                // iterate items in list view, to find 'TriggerName'
                for (int i = 0; i < lvTriggerAreas.Items.Count; i++)
                {
                    ListViewItem listViewItem = lvTriggerAreas.Items[i];
                    if (listViewItem == null) continue;

                    if (listViewItem.Text == triggerName)
                    {
                        listViewItem.BackColor = Color.Yellow;
                        listViewItem.Selected = true;
                    }
                    else
                    {
                        listViewItem.BackColor = Color.White;
                        listViewItem.Selected = false;
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("SelectTriggerAreaInListView method threw the exception " + ex.Message);
#endif
            }
        }


        // 9/29/2009
        /// <summary>
        /// Deletes all 'SelectedItems' for the given ListView, which the user has chosen to delete.
        /// </summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // make sure an item is selected from list
                if (lvTriggerAreas.SelectedItems.Count == 0)
                {
                    lblTriggerErrorMessages.Text = @"You MUST select an item first, in order to 'Delete' it.";
                    return;
                }

                // Iterate selectedItem, and delete from list and Dictionary
                while (lvTriggerAreas.SelectedItems.Count > 0)
                {
                    // 1st - delete from dictionary
                    TerrainTriggerAreas.TriggerAreas.Remove(lvTriggerAreas.SelectedItems[0].Text);

                    // 2nd - delete from listView
                    lvTriggerAreas.Items.Remove(lvTriggerAreas.SelectedItems[0]);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnDelete_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 9/29/2009
        /// <summary>
        /// Event handler to capture the Enter event and then populate the 
        /// list view with the records from the TerrainTriggerAreas class.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabAreas_Enter(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                PopulateTriggerAreasListView();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("tabAreas_Enter method threw the exception " + ex.Message);
#endif
            }
        }

        // 5/17/2012
        /// <summary>
        ///  Event handler which occurs when the user presses the 'Set' button for the Name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetName_Click(object sender, EventArgs e)
        {
            // Check if Valid name
            if (!ValidateUserDefinedName())
                return;

            // Set new Name
            SetUserDefinedName();
        }

        // 5/17/2012
        /// <summary>
        /// Sets the user's defined name.
        /// </summary>
        private void SetUserDefinedName()
        {
            try // 6/22/2010
            {
                // 5/17/2012 - Set 'InstancedItemPickedIndex' when ScenaryItems.
                var scenaryItemScene = _currentSceneItem as ScenaryItemScene;
                if (scenaryItemScene != null)
                {
                    scenaryItemScene.InstancedItemPickedIndex = _instancedItemPickedIndex;
                }

                // update the field back to the linked sceneItem.
                _currentSceneItem.Name = txtUserDefinedName.Text;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("txtUserDefinedName_TextChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 5/17/2012
        /// <summary>
        /// Event handler to capture the Name field Validated event, which will
        /// check if the given 'Name' is valid; to be Valid, the name must
        /// not already exist for another item.
        /// </summary>
        private bool ValidateUserDefinedName()
        {
            try // 6/22/2010
            {
                // if Null string, return.
                if (string.IsNullOrEmpty(txtUserDefinedName.Text))
                    return false;

                // check if 'Name' exist in the Player's Dictionary
                if (Player.SceneItemsByName.ContainsKey(txtUserDefinedName.Text))
                {
                    // Let user know error occurred.
                    errorProviderName.SetError(txtUserDefinedName, "Name given must be unique!");

                    // clear out name, since not valid.
                    _currentSceneItem.Name = string.Empty;
                    txtUserDefinedName.Text = string.Empty;

                    return false;
                }

                // Clear the error, if any, in the error provider.
                errorProviderName.SetError(txtUserDefinedName, "");

                return true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("txtUserDefinedName_Validated method threw the exception " + ex.Message);
#endif
                return false;
            }
        }

        // 10/13/2009
        /// <summary>
        /// When user selects a record in the Waypoints list, the location 
        /// is retrieved from the AStarItem's Dictionary to be displayed, and
        /// the Camera position is moved to this waypoint location.
        /// </summary>
        private void lvWaypoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // 4/7/2010 - Check if SelectedIndex count is greater than 0, since this can fire
                //            when user moves away from selection!
                if (lvWaypoints.SelectedItems.Count <= 0) return;

                // Retrieve given way point's location from the AStarItem Dictionary, 
                // and display to user.
                int index = GetWaypointIndex();

                // 4/7/2010 - If -1 index, then just return.
                if (index == -1) return;

                Vector3 waypointLocation;
                if (!TerrainWaypoints.GetExistingWaypoint(index, out waypointLocation)) return;

                // Update display with location
                SetWaypointLocationText(ref waypointLocation);
                // Move the Camera's Target to this position
                Camera.CameraTarget = new Vector3(waypointLocation.X, 0, waypointLocation.Z); // 4/13/2010
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("lvWaypoints_SelectedIndexChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/13/2009
        /// <summary>
        /// Helper method, which extracts the index from the text of the waypoint in the list view.
        /// </summary>
        /// <returns>Index value as int.</returns>
        private int GetWaypointIndex()
        {
            try
            {
                // 4/7/2010 - Verify has items.
                if (lvWaypoints.SelectedItems.Count < 1)
                    return -1;

                return Convert.ToInt32(lvWaypoints.SelectedItems[0].Text.TrimStart("Waypoint ".ToCharArray()));
            }
            catch (ArgumentOutOfRangeException)
            {
                return -1;
            }
        }


        // 10/13/2009
        /// <summary>
        /// Helper method, which sets the internal X-Y-Z text fields, with
        /// given data from the waypointLocation Vector3 given. 
        /// </summary>
        /// <param name="waypointLocation">Waypoint Location</param>
        public void SetWaypointLocationText(ref Vector3 waypointLocation)
        {
            try // 6/22/2010
            {
                // Update display with location
                nupWayPt_X.Value = (decimal) (waypointLocation.X);
                nupWayPt_Y.Value = (decimal) (waypointLocation.Y);
                nupWayPt_Z.Value = (decimal) (waypointLocation.Z);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("SetWaypointLocationText method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/13/2009
        /// <summary>
        /// Helper method, which adds the given waypoint index, to
        /// the internal List view.
        /// </summary>
        /// <param name="waypointIndex">Waypoint Index</param>
        public void AddWaypointIndexToListview(int waypointIndex)
        {
            try
            {
                // Add location to ListView for Waypoints
                lvWaypoints.Items.Add("Waypoint " + waypointIndex);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("AddWaypointIndexToListview method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/13/2009
        /// <summary>
        /// Manually adds a new waypoint, using the X-Y-Z location
        /// data given in the text fields.  
        /// </summary>
        private void btnAddWaypoint_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // retrieve X-Y-Z field data
                Vector3 waypointLocation;
                waypointLocation.X = (float) (nupWayPt_X.Value);
                waypointLocation.Y = (float) (nupWayPt_Y.Value);
                waypointLocation.Z = (float) (nupWayPt_Z.Value);

                // Add to Dictionary
                int index = TerrainWaypoints.AddWaypoint(ref waypointLocation);

                // Add location to ListView for Waypoints
                AddWaypointIndexToListview(index);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnAddWaypoint_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/13/2009
        /// <summary>
        /// EventHandler to capture the click of the PopulateList button, which will then
        /// populate the list with current 'Waypoints'.
        /// </summary>
        private void btnPopulateWaypoints_Click(object sender, EventArgs e)
        {
            PopulateWaypointsListView();
        }


        // 10/13/2009
        /// <summary>
        /// Helper method, which populates the Waypoints ListView with the given records from the
        /// TerrainWaypoints class.
        /// </summary>
        private void PopulateWaypointsListView()
        {
            try // 6/22/2010
            {
                if (TerrainWaypoints.Waypoints.Count <= 0 || lvWaypoints == null) return;

                // iterate dictionary
                lvWaypoints.Clear();
                foreach (var waypoint in TerrainWaypoints.Waypoints)
                {
                    // Addwaypoint Index.
                    AddWaypointIndexToListview(waypoint.Key);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PopulateWaypointsListView method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/16/2009
        /// <summary>
        /// Helper method, which populates the WaypointPaths ComboBox with the given records from the
        /// TerrainWaypoints class.
        /// </summary>
        private void PopulateWaypointsPathComboBox()
        {
            try // 6/22/2010
            {
                if (TerrainWaypoints.WaypointPaths.Count <= 0 || lvWaypointsPaths == null) return;

                // iterate dictionary
                lvWaypointsPaths.Clear();
                cbWaypointPathNames.Items.Clear();
                foreach (var waypointPath in TerrainWaypoints.WaypointPaths)
                {
                    // Add WaypointPath Name to ComboBox.
                    cbWaypointPathNames.Items.Add(waypointPath.Key);
                } // End ForEach
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PopulateWaypointsPathComboBox method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/13/2009
        /// <summary>
        /// Deletes all 'SelectedItems' for the given ListView, which the user has chosen to delete.
        /// </summary>
        private void btnDeleteWaypoint_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // make sure an item is selected from list
                if (lvWaypoints.SelectedItems.Count == 0)
                {
                    lblWaypointsErrorMessages.Text = @"You MUST select an item first, in order to 'Delete' it.";
                    return;
                }

                // Iterate selectedItem, and delete from list and Dictionary
                while (lvWaypoints.SelectedItems.Count > 0)
                {
                    // 1st - delete from dictionary
                    int index = GetWaypointIndex();
                    TerrainWaypoints.Waypoints.Remove(index);

                    // 2nd - delete from WaypointPath linked list, if exist in any. (4/7/2010)
                    TerrainWaypoints.DeleteWaypointFromAllWaypointPaths(index);

                    // 3rd - delete from listView
                    lvWaypoints.Items.Remove(lvWaypoints.SelectedItems[0]);
                }

                // 4/7/2010 - Update ListView, to capture any possible WayPointPath changes.
                UpdateWaypointPathsListView();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnDeleteWaypoint_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/14/2009
        /// <summary>
        /// Searches the given ListView for the 'Waypoint' index, and if found, highlights
        /// backColor to Yellow, and marks as 'Selected' item.
        /// </summary>
        /// <param name="waypointIndex">Waypoint Index</param>
        public void SelectWaypointsInListView(int waypointIndex)
        {
            try // 6/22/2010
            {
                // Create Waypoint name
                // ReSharper disable RedundantToStringCall
                string waypointName = "Waypoint " + waypointIndex.ToString(CultureInfo.InvariantCulture);
                // ReSharper restore RedundantToStringCall

                // iterate items in list view, to find 'Waypoint' index.
                int count = lvWaypoints.Items.Count; // 5/28/2010
                for (int i = 0; i < count; i++)
                {
                    ListViewItem listViewItem = lvWaypoints.Items[i];
                    if (listViewItem == null) continue;

                    if (listViewItem.Text == waypointName)
                    {
                        listViewItem.BackColor = Color.Yellow;
                        listViewItem.Selected = true;
                    }
                    else
                    {
                        listViewItem.BackColor = Color.White;
                        listViewItem.Selected = false;
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("SelectWaypointsInListView method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/14/2009
        /// <summary>
        /// Event handler to capture the Enter event and then populate the 
        /// list view with the records from the TerrainWaypoints class.
        /// </summary>
        private void tabWaypoints_Enter(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                PopulateWaypointsListView();

                // 10/16/2009 - Populate the Waypoints PathNames.
                PopulateWaypointsPathComboBox();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("tabWaypoints_Enter method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/15/2009
        /// <summary>
        /// Event handler to capture the Add 'Waypoints' PathName, and add it to
        /// the ComboBox 'WaypointPathNames'.
        /// </summary>
        private void btnAddWaypointsPathName_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // 4/8/2010 - Check if NullorEmpty string; occurs when Validation stops entry, and send
                //            null string instead!
                var selectedText = txtWaypointsPathName.Text;
                if (string.IsNullOrEmpty(selectedText)) return;

                // Add new PathName to the ComboBox.
                var index = cbWaypointPathNames.Items.Add(selectedText);

                // 4/8/2010 - Set to new pathName just added.
                cbWaypointPathNames.SelectedIndex = index;

                // Clear out PathName text box.
                txtWaypointsPathName.Text = string.Empty;

                // Clear ListView
                lvWaypointsPaths.Clear();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnAddWaypointsPathName_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/15/2009
        /// <summary>
        /// Event handler to capture the '=>' arrow add, which adds all 'SelectedItems'
        /// from the ListView 'lvWaypoints', to the ListView 'lvWaypointsPaths'.
        /// </summary>
        private void btnAddToPathList_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // make sure 'SelectedItems' exist for this to work.
                if (lvWaypoints.SelectedItems.Count == 0)
                {
                    lblWaypointsErrorMessages.Text = @"There was no 'Selected' waypoints to copy over; try again.";
                    return;
                }

                // make sure the comboBox is not empty
                if (cbWaypointPathNames.Items.Count == 0)
                {
                    lblWaypointsErrorMessages.Text =
                        @"You must define at least 1 Waypoint PathName, and then select this in the drop-down ComboBox.";
                    return;
                }

                // 2/1/2010 - Make sure 'cbWaypointPathNames.SelectedItem' is not null.
                if (cbWaypointPathNames.SelectedItem == null)
                {
                    lblWaypointsErrorMessages.Text = @"There was no 'Selected' waypoints to copy over; try again.";
                    return;
                }

                // Clear any error messages
                lblWaypointsErrorMessages.Text = "";

                // Get WaypointsPaths LinkedList from Dictionary
                string waypointPathName = cbWaypointPathNames.SelectedItem.ToString(); // 4/7/2010
                ListView.SelectedListViewItemCollection selectedItems = lvWaypoints.SelectedItems; // 4/7/2010

                // 4/7/2010 - Refactored code out, to new method.
                TerrainWaypoints.UpdateVisualPath_AddSelectedItems(this, waypointPathName, selectedItems,
                                                                   lvWaypointsPaths);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnAddToPathList_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/15/2009
        /// <summary>
        /// Event handler to capture the PathName field Validated event, which will
        /// check if the given 'PathName' is valid; to be Valid, the name must
        /// not already exist for another item.
        /// </summary>
        private void txtWaypointsPathName_Validated(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // 6/24/2012 - Validate new waypointpath name.
                TerrainWaypoints.ValidateWaypointsPathName(txtWaypointsPathName.Text);

                // Clear the error, if any, in the error provider.
                errorProviderName.SetError(txtWaypointsPathName, "");

                // Store new 'WaypointPathName' into WaypointPaths Dictionary
                TerrainWaypoints.CreateEmptyWaypointPathInDictionary(txtWaypointsPathName.Text); // 6/24/2012
            }
            catch (InvalidOperationException)
            {
                // Let user know error occurred.
                errorProviderName.SetError(txtWaypointsPathName, "Name given must be unique!");

                // clear out name, since not valid.
                txtWaypointsPathName.Text = string.Empty;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("txtWaypointsPathName_Validated method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/15/2009
        /// <summary>
        /// Event handler to capture the SelectedIndex change of the Waypoint comboBox; this
        /// updates the ListView with relevant waypoints for the chosen PathName.
        /// </summary>
        private void cbWaypointPathNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            // clear listView
            UpdateWaypointPathsListView();
        }

        // 4/7/2010
        /// <summary>
        /// Updates the ListView for 'WaypointPaths', with the WaypointPath name, selected in the 
        /// drop-down combo box 'WaypointPathNames'.
        /// </summary>
        private void UpdateWaypointPathsListView()
        {
            try // 6/22/2010
            {
                lvWaypointsPaths.Clear();

                // 4/9/2010 - Verify there is a 'PathName' to check.
                if (cbWaypointPathNames.SelectedItem == null) return;
                var waypointPathName = cbWaypointPathNames.SelectedItem.ToString();
                if (string.IsNullOrEmpty(waypointPathName))
                    return;

                // Get WaypointsPaths LinkedList from Dictionary
                var linkedList = TerrainWaypoints.WaypointPaths[waypointPathName];

                // iterate LinkedList and add items to ListView.
                foreach (var item in linkedList)
                {
                    // Add to ListView
                    lvWaypointsPaths.Items.Add("Waypoint " + item);
                }

                // 10/16/2009 - Create Visual LineStrip
                TerrainWaypoints.CreateVisualPathLineStripForPathName(waypointPathName);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("UpdateWaypointPathsListView method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/15/2009 - The LVItem being dragged

        // 10/15/2009
        /// <summary>
        /// Handles the MouseDown event, and starts the Drag-Drop reorder 
        /// for the WaypointsPaths ListView, by retrieving the ListViewItem
        /// which is currently at the Mouse Cursor position.
        /// </summary>
        private void lvWaypointsPaths_MouseDown(object sender, MouseEventArgs e)
        {
            try // 6/22/2010
            {
                // Do Drag Op, ONLY when the check box is enabled!
                if (cbReorderWaypoints.CheckState != CheckState.Checked)
                    return;

                // Gets the ListViewItem at the Mouse X-Y position.
                _itemDnD = lvWaypointsPaths.GetItemAt(e.X, e.Y);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("lvWaypointsPaths_MouseDown method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/15/2009
        /// <summary>
        /// Handles the MouseMove event, by checking if a ListView item is
        /// currently being dragged, and if so, to show the Hand cursor.
        /// </summary>
        private void lvWaypointsPaths_MouseMove(object sender, MouseEventArgs e)
        {
            try // 6/22/2010
            {
                if (_itemDnD == null || cbReorderWaypoints.CheckState != CheckState.Checked)
                {
                    Cursor = Cursors.Default;
                    return;
                }

                // Show the user that a drag operation is occurring.
                Cursor = Cursors.Hand;

                // calculate the bottom of the last item in the LV so that you don't have to stop your drag at the last item
                var lastItemBottom = Math.Min(e.Y,
                                              lvWaypointsPaths.Items[lvWaypointsPaths.Items.Count - 1].GetBounds(
                                                  ItemBoundsPortion.Entire).Bottom - 1);

                // use 0 instead of e.X so that you don't have to keep inside the columns while dragging
                var itemOver = lvWaypointsPaths.GetItemAt(0, lastItemBottom);

                if (itemOver == null)
                    return;

                System.Drawing.Rectangle rc = itemOver.GetBounds(ItemBoundsPortion.Entire);
                if (e.Y < rc.Top + (rc.Height/2))
                {
                    lvWaypointsPaths.LineBefore = itemOver.Index;
                    lvWaypointsPaths.LineAfter = -1;
                }
                else
                {
                    lvWaypointsPaths.LineBefore = -1;
                    lvWaypointsPaths.LineAfter = itemOver.Index;
                }

                // invalidate the LV so that the insertion line is shown
                lvWaypointsPaths.Invalidate();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("lvWaypointsPaths_MouseMove method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/15/2009
        /// <summary>
        /// Handles the MouseUp event, by checking if a LiewView item is
        /// currently being dropped, and if so, checks on which item to
        /// insert the node at; either before or after the picked node.
        /// </summary>
        private void lvWaypointsPaths_MouseUp(object sender, MouseEventArgs e)
        {
            try // 6/22/2010
            {
                // use 0 instead of e.X so that you don't have
                // to keep inside the columns while dragging
                var itemTarget = lvWaypointsPaths.GetItemAt(0, e.Y);

                if (itemTarget == null || cbReorderWaypoints.CheckState != CheckState.Checked)
                    return;

                System.Drawing.Rectangle rc = itemTarget.GetBounds(ItemBoundsPortion.Entire);

                // find out if we insert before or after the item the mouse is over
                var insertBeforeTarget = e.Y < rc.Top + (rc.Height/2);

                if (_itemDnD == itemTarget) return;

                // get waypointIndex to itemOver ListViewItem.
                var itemTargetWaypointIndex = Convert.ToInt32(itemTarget.Text.TrimStart("Waypoint ".ToCharArray()));
                // get waypointIndex of item to Move
                var itemToMoveWaypointIndex = Convert.ToInt32(_itemDnD.Text.TrimStart("Waypoint ".ToCharArray()));

                // Reorder the Nodes
                if (insertBeforeTarget)
                {
                    lvWaypointsPaths.Items.Remove(_itemDnD);
                    lvWaypointsPaths.Items.Insert(itemTarget.Index, _itemDnD);

                    // Update the LinkedList to reorder.
                    TerrainWaypoints.ReorderPathWaypointToBeBefore(cbWaypointPathNames.Text, itemToMoveWaypointIndex,
                                                                   itemTargetWaypointIndex);
                }
                    // no, insert after target.
                else
                {
                    lvWaypointsPaths.Items.Remove(_itemDnD);
                    lvWaypointsPaths.Items.Insert(itemTarget.Index + 1, _itemDnD);

                    // Update the LinkedList to reorder.
                    TerrainWaypoints.ReorderPathWaypointToBeAfter(cbWaypointPathNames.Text, itemToMoveWaypointIndex,
                                                                  itemTargetWaypointIndex);
                }

                // clear the insertion line
                lvWaypointsPaths.LineAfter = lvWaypointsPaths.LineBefore = -1;
                lvWaypointsPaths.Invalidate();

                // Clear pointer to item
                _itemDnD = null;
                Cursor = Cursors.Default;

                // 10/16/2009 - Update to the Path Striplist
                TerrainWaypoints.CreateVisualPathLineStripForPathName(cbWaypointPathNames.Text);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("lvWaypointsPaths_MouseUp method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/15/2009
        /// <summary>
        /// Event handler to capture the 'Delete' action, which will delete the
        /// selectedItem from the ListView and the LinkedList associated to the
        /// given WaypointsPath.
        /// </summary>
        private void btnDeleteWaypointsPath_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Check if some item was selected, in order to do the deletes.
                var count = lvWaypointsPaths.SelectedItems.Count;
                if (count == 0)
                {
                    lblWaypointsErrorMessages.Text =
                        @"In order to delete a waypoint from the list, you must select it first.";
                    return;
                }

                // Clear any errors
                lblWaypointsErrorMessages.Text = string.Empty;

                // 4/7/2010 - Refactor code to now be in the TerrainWaypoints class.
                TerrainWaypoints.UpdateVisualPath_DeleteSelectedItems(this, cbWaypointPathNames.Text, lvWaypointsPaths);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnDeleteWaypointsPath_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/15/2009
        /// <summary>
        /// Event handler for the ReorderWaypoints check box, which turns on or off the
        /// ability to Drag-N-Drop the waypoint items in the ListView box.
        /// </summary>
        private void cbReorderWaypoints_CheckedChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Disables/Enables the Delete button.
                switch (cbReorderWaypoints.CheckState)
                {
                    case CheckState.Unchecked:
                        btnDeleteWaypointsPath.Enabled = true;
                        break;
                    case CheckState.Checked:
                        btnDeleteWaypointsPath.Enabled = false;
                        break;
                    case CheckState.Indeterminate:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("cbReorderWaypoints_CheckedChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/20/2009; // 1/15/2011 - Updated to Add/Remove Player instances correctly.
        /// <summary>
        /// EventHandler to capture the value change of PlayerNumber and 
        /// update to the current linked sceneItem.
        /// </summary>
        private void nudPlayerNumber_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                if (_resettingValues) return;

                // 1/16/2011 - Transfer SceneItem to proper Player instance.
                Player.TransferSelectableItem((SceneItemWithPick) _currentSceneItem, (byte) nudPlayerNumber.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nudPlayerNumber_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 12/7/2009
        /// <summary>
        /// Captures the close event, and cancels the close action; instead, the form is simply hidden
        /// from view by setting the 'Visible' flag to False.  This fixes the issue of the HEAP crash 
        /// when trying to re instantiate a form, after it was already created in the 'TerrainEditRoutines' class.
        /// </summary>
        private void PropertiesTools_FormClosing(object sender, FormClosingEventArgs e)
        {
            try // 6/22/2010
            {
                e.Cancel = true;
                Visible = false;

                // deactivate updating in TerrainTriggerAreas
                TerrainShape.TriggerAreas.Enabled = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PropertiesTools_FormClosing method threw the exception " + ex.Message);
#endif
            }
        }

        #region Materials Tab Methods

        // 2/4/2010
        /// <summary>
        /// Method helper, used to retrieve the 'InstancedModel' and its ShapeItem
        /// as casted interface 'IInstancedItem'.
        /// </summary>
        /// <param name="instancedModel">(OUT) <see cref="InstancedModel"/> instance</param>
        /// <param name="instancedItem">(OUT) <see cref="IInstancedItem"/> instance </param>
        /// <returns>InstancedModel's Shape cast as IInstancedItem interface.</returns>
        internal bool GetInstancedModelAndInstancedItem(out InstancedModel instancedModel,
                                                        out IInstancedItem instancedItem)
        {
            // Set to null
            instancedModel = null;
            instancedItem = null;

            try // 6/22/2010
            {
                if (_currentSceneItem == null)
                {
                    lblMaterialsErrorMessage.Text =
                        @"You must first pick some 'SceneItem' from game world, before trying to assign a material.";

                    return false;
                }

                // ReSharper disable RedundantCast
                instancedItem = (_currentSceneItem.ShapeItem as IInstancedItem);
                // ReSharper restore RedundantCast
                if (instancedItem == null) return false;

                // 8/13/2009 - Cache
                instancedModel = InstancedItem.InstanceModels[(int) instancedItem.ItemType];
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("GetInstancedModelAndInstancedItem method threw the exception " + ex.Message);
#endif
            }

            return instancedModel != null;
        }

        // 2/3/2010
        /// <summary>
        /// EventHandler, used to assign the given 'ProceduralMaterialId' to the 
        /// current picked 'SceneItem'.
        /// </summary>
        private void btnAssignMaterialId_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Check if anything picked yet.
                InstancedModel instancedModel;
                IInstancedItem instancedItem;
                if (!GetInstancedModelAndInstancedItem(out instancedModel, out instancedItem))
                    return;

                // Read current MatieralId from NUP control.
                var materialId = (int) nupMaterialId.Value;

                // 2/8/2010 - Get ModelPart index key
                var modelPartIndexKey = (int) cmbPickByPart.SelectedValue;

                // then just assign per batch, the default way.
                instancedModel.AssignProceduralMaterialId(materialId, modelPartIndexKey);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnAssignMaterialId_Click method threw the exception " + ex.Message);
#endif
            }
        }


        // 2/4/2010
        /// <summary>
        /// EventHandler for (A) button, used to assign the given parameter values.
        /// </summary>
        private void btnAssignDiffuseColor_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Check if anything picked yet.
                InstancedModel instancedModel;
                IInstancedItem instancedItem;
                if (!GetInstancedModelAndInstancedItem(out instancedModel, out instancedItem))
                    return;

                // Collect new value
                var newValue = new Microsoft.Xna.Framework.Color
                                   {
                                       R = (byte) nupDiffuseColor_r.Value,
                                       G = (byte) nupDiffuseColor_g.Value,
                                       B = (byte) nupDiffuseColor_b.Value,
                                       A = (byte) nupDiffuseColor_a.Value
                                   };

                // 2/8/2010 - Get ModelPart index key
                var modelPartIndexKey = (int) cmbPickByPart.SelectedValue;


                instancedModel.SetProceduralMaterialParameter(ProceduralMaterialParameters.DiffuseColor, newValue,
                                                              modelPartIndexKey);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnAssignDiffuseColor_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 2/4/2010
        /// <summary>
        /// EventHandler for (A) button, used to assign the given parameter values.
        /// </summary>
        private void btnAssignSpecularColor_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Check if anything picked yet.
                InstancedModel instancedModel;
                IInstancedItem instancedItem;
                if (!GetInstancedModelAndInstancedItem(out instancedModel, out instancedItem))
                    return;

                // Collect new value
                var newValue = new Microsoft.Xna.Framework.Color
                                   {
                                       R = (byte) nupSpecularColor_r.Value,
                                       G = (byte) nupSpecularColor_g.Value,
                                       B = (byte) nupSpecularColor_b.Value,
                                       A = (byte) nupSpecularColor_a.Value
                                   };

                // 2/8/2010 - Get ModelPart index key
                var modelPartIndexKey = (int) cmbPickByPart.SelectedValue;

                instancedModel.SetProceduralMaterialParameter(ProceduralMaterialParameters.SpecularColor, newValue,
                                                              modelPartIndexKey);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnAssignSpecularColor_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 2/4/2010
        /// <summary>
        /// EventHandler for (A) button, used to assign the given parameter values.
        /// </summary>
        private void btnAssignMiscColor_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Check if anything picked yet.
                InstancedModel instancedModel;
                IInstancedItem instancedItem;
                if (!GetInstancedModelAndInstancedItem(out instancedModel, out instancedItem))
                    return;

                // Collect new value
                var newValue = new Microsoft.Xna.Framework.Color
                                   {
                                       R = (byte) nupMiscColor_r.Value,
                                       G = (byte) nupMiscColor_g.Value,
                                       B = (byte) nupMiscColor_b.Value,
                                       A = (byte) nupMiscColor_a.Value
                                   };

                // 2/8/2010 - Get ModelPart index key
                var modelPartIndexKey = (int) cmbPickByPart.SelectedValue;

                instancedModel.SetProceduralMaterialParameter(ProceduralMaterialParameters.MiscColor, newValue,
                                                              modelPartIndexKey);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnAssignMiscColor_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 2/4/2010
        /// <summary>
        /// EventHandler for (A) button, used to assign the given parameter values.
        /// </summary>
        private void btnAssignMiscFloat1_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Check if anything picked yet.
                InstancedModel instancedModel;
                IInstancedItem instancedItem;
                if (!GetInstancedModelAndInstancedItem(out instancedModel, out instancedItem))
                    return;

                // 2/8/2010 - Get ModelPart index key
                var modelPartIndexKey = (int) cmbPickByPart.SelectedValue;

                instancedModel.SetProceduralMaterialParameter(ProceduralMaterialParameters.MiscFloat1,
                                                              (float) nupMiscFloat1.Value, modelPartIndexKey);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnAssignMiscFloat1_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 2/4/2010
        /// <summary>
        /// EventHandler for (A) button, used to assign the given parameter values.
        /// </summary>
        private void btnAssignMiscFloat2_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Check if anything picked yet.
                InstancedModel instancedModel;
                IInstancedItem instancedItem;
                if (!GetInstancedModelAndInstancedItem(out instancedModel, out instancedItem))
                    return;

                // 2/8/2010 - Get ModelPart index key
                var modelPartIndexKey = (int) cmbPickByPart.SelectedValue;

                instancedModel.SetProceduralMaterialParameter(ProceduralMaterialParameters.MiscFloat2,
                                                              (float) nupMiscFloat2.Value, modelPartIndexKey);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnAssignMiscFloat2_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 2/4/2010
        /// <summary>
        /// EventHandler for (A) button, used to assign the given parameter values.
        /// </summary>
        private void btnAssignMiscFloat3_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Check if anything picked yet.
                InstancedModel instancedModel;
                IInstancedItem instancedItem;
                if (!GetInstancedModelAndInstancedItem(out instancedModel, out instancedItem))
                    return;

                // 2/8/2010 - Get ModelPart index key
                var modelPartIndexKey = (int) cmbPickByPart.SelectedValue;

                instancedModel.SetProceduralMaterialParameter(ProceduralMaterialParameters.MiscFloat3,
                                                              (float) nupMiscFloat3.Value, modelPartIndexKey);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnAssignMiscFloat3_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 2/4/2010
        /// <summary>
        /// EventHandler for (A) button, used to assign the given parameter values.
        /// </summary>
        private void btnAssignMiscFloat4_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Check if anything picked yet.
                InstancedModel instancedModel;
                IInstancedItem instancedItem;
                if (!GetInstancedModelAndInstancedItem(out instancedModel, out instancedItem))
                    return;

                // 2/8/2010 - Get ModelPart index key
                var modelPartIndexKey = (int) cmbPickByPart.SelectedValue;

                instancedModel.SetProceduralMaterialParameter(ProceduralMaterialParameters.MiscFloat4,
                                                              (float) nupMiscFloat4.Value, modelPartIndexKey);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnAssignMiscFloat4_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 2/4/2010
        /// <summary>
        /// EventHandler for (A) button, used to assign the given parameter values.
        /// </summary>
        private void btnAssignMiscFloat5_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Check if anything picked yet.
                InstancedModel instancedModel;
                IInstancedItem instancedItem;
                if (!GetInstancedModelAndInstancedItem(out instancedModel, out instancedItem))
                    return;

                // Collect new value
                var newValue = new Vector2
                                   {
                                       X = (float) nupMiscFloatx2_5_x.Value,
                                       Y = (float) nupMiscFloatx2_5_y.Value,
                                   };

                // 2/8/2010 - Get ModelPart index key
                var modelPartIndexKey = (int) cmbPickByPart.SelectedValue;

                instancedModel.SetProceduralMaterialParameter(ProceduralMaterialParameters.MiscFloatx2_5, newValue,
                                                              modelPartIndexKey);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnAssignMiscFloat5_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 2/4/2010
        /// <summary>
        /// EventHandler for (A) button, used to assign the given parameter values.
        /// </summary>
        private void btnAssignMiscFloat6_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Check if anything picked yet.
                InstancedModel instancedModel;
                IInstancedItem instancedItem;
                if (!GetInstancedModelAndInstancedItem(out instancedModel, out instancedItem))
                    return;

                // Collect new value
                var newValue = new Vector4
                                   {
                                       X = (float) nupMiscFloatx4_6_x.Value,
                                       Y = (float) nupMiscFloatx4_6_y.Value,
                                       Z = (float) nupMiscFloatx4_6_z.Value,
                                       W = (float) nupMiscFloatx4_6_w.Value
                                   };

                // 2/8/2010 - Get ModelPart index key
                var modelPartIndexKey = (int) cmbPickByPart.SelectedValue;

                instancedModel.SetProceduralMaterialParameter(ProceduralMaterialParameters.MiscFloatx4_6, newValue,
                                                              modelPartIndexKey);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnAssignMiscFloat6_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 2/4/2010
        /// <summary>
        /// EventHandler for (A) button, used to assign the given parameter values.
        /// </summary>
        private void btnAssignMiscFloat7_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Check if anything picked yet.
                InstancedModel instancedModel;
                IInstancedItem instancedItem;
                if (!GetInstancedModelAndInstancedItem(out instancedModel, out instancedItem))
                    return;

                // Collect new value
                var newValue = new Vector4
                                   {
                                       X = (float) nupMiscFloatx4_7_x.Value,
                                       Y = (float) nupMiscFloatx4_7_y.Value,
                                       Z = (float) nupMiscFloatx4_7_z.Value,
                                       W = (float) nupMiscFloatx4_7_w.Value
                                   };

                // 2/8/2010 - Get ModelPart index key
                var modelPartIndexKey = (int) cmbPickByPart.SelectedValue;

                instancedModel.SetProceduralMaterialParameter(ProceduralMaterialParameters.MiscFloatx4_7, newValue,
                                                              modelPartIndexKey);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnAssignMiscFloat7_Click method threw the exception " + ex.Message);
#endif
            }
        }

        // 2/8/2010
        /// <summary>
        /// Method helper, to specifically update the ComboBox 'PickByPart', with
        /// the picked instanced model's PartName and IndexKey.
        /// </summary>
        private void PopulateComboBox_PickByPart()
        {
            try // 6/22/2010
            {
                // Check if anything picked yet.
                InstancedModel instancedModel;
                IInstancedItem instancedItem;
                if (!GetInstancedModelAndInstancedItem(out instancedModel, out instancedItem))
                    return;

                // Retrieve ModelPart names list.
                List<InstancedModel.ModelPartNames> modelPartNames;
                instancedModel.GetModelPartNamesWithIndexes(out modelPartNames);

                // check if NULL
                if (modelPartNames == null) return;

                // Bind ComboBox
                cmbPickByPart.DataSource = modelPartNames;
                cmbPickByPart.DisplayMember = "Name"; // Set to Public Property in my struct node.
                cmbPickByPart.ValueMember = "IndexKey";
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PopulateComboBox_PickByPart method threw the exception " + ex.Message);
#endif
            }
        }


        // 2/9/2010
        /// <summary>
        /// Creates the MaterialParameters list, by adding each material parameter's 'MaterialControls' 
        /// group node, which defines the references to each of PropertiesTool's controls, like the label control
        /// or button control for a given material parameter.
        /// </summary>
        private void CreateAndPopulateMaterialParamRefs()
        {
            try // 6/22/2010
            {
                // Create and add the 10 generic material parameters
                PropertiesToolsMaterialParams.ProceduralMaterialParams = new MaterialParameters();
                PropertiesToolsMaterialParams.ProceduralMaterialParams.AddMaterialControlsGroup(
                    ProceduralMaterialParameters.DiffuseColor,
                    new MaterialControls(lblDiffseColor, btnAssignDiffuseColor, nupDiffuseColor_r, nupDiffuseColor_g,
                                         nupDiffuseColor_b, nupDiffuseColor_a));
                PropertiesToolsMaterialParams.ProceduralMaterialParams.AddMaterialControlsGroup(
                    ProceduralMaterialParameters.SpecularColor,
                    new MaterialControls(lblSpecularColor, btnAssignSpecularColor, nupSpecularColor_r,
                                         nupSpecularColor_g, nupSpecularColor_b, nupSpecularColor_a));
                PropertiesToolsMaterialParams.ProceduralMaterialParams.AddMaterialControlsGroup(
                    ProceduralMaterialParameters.MiscColor,
                    new MaterialControls(lblMiscColor, btnAssignMiscColor, nupMiscColor_r, nupMiscColor_g,
                                         nupMiscColor_b, nupMiscColor_a));
                PropertiesToolsMaterialParams.ProceduralMaterialParams.AddMaterialControlsGroup(
                    ProceduralMaterialParameters.MiscFloat1,
                    new MaterialControls(lblMiscFloat1, btnAssignMiscFloat1, nupMiscFloat1));
                PropertiesToolsMaterialParams.ProceduralMaterialParams.AddMaterialControlsGroup(
                    ProceduralMaterialParameters.MiscFloat2,
                    new MaterialControls(lblMiscFloat2, btnAssignMiscFloat2, nupMiscFloat2));
                PropertiesToolsMaterialParams.ProceduralMaterialParams.AddMaterialControlsGroup(
                    ProceduralMaterialParameters.MiscFloat3,
                    new MaterialControls(lblMiscFloat3, btnAssignMiscFloat3, nupMiscFloat3));
                PropertiesToolsMaterialParams.ProceduralMaterialParams.AddMaterialControlsGroup(
                    ProceduralMaterialParameters.MiscFloat4,
                    new MaterialControls(lblMiscFloat4, btnAssignMiscFloat4, nupMiscFloat4));
                PropertiesToolsMaterialParams.ProceduralMaterialParams.AddMaterialControlsGroup(
                    ProceduralMaterialParameters.MiscFloatx2_5,
                    new MaterialControls(lblMiscFloatx2_5, btnAssignMiscFloat5, nupMiscFloatx2_5_x, nupMiscFloatx2_5_y));
                PropertiesToolsMaterialParams.ProceduralMaterialParams.AddMaterialControlsGroup(
                    ProceduralMaterialParameters.MiscFloatx4_6,
                    new MaterialControls(lblMiscFloatx4_6, btnAssignMiscFloat6, nupMiscFloatx4_6_x, nupMiscFloatx4_6_y,
                                         nupMiscFloatx4_6_z, nupMiscFloatx4_6_w));
                PropertiesToolsMaterialParams.ProceduralMaterialParams.AddMaterialControlsGroup(
                    ProceduralMaterialParameters.MiscFloatx4_7,
                    new MaterialControls(lblMiscFloatx4_7, btnAssignMiscFloat7, nupMiscFloatx4_7_x, nupMiscFloatx4_7_y,
                                         nupMiscFloatx4_7_z, nupMiscFloatx4_7_w));
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("CreateAndPopulateMaterialParamRefs method threw the exception " + ex.Message);
#endif
            }
        }

        // 2/9/2010
        /// <summary>
        /// EventHandler to capture when a user updates the MaterialId control.
        /// </summary>
        private void nupMaterialId_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // ONLY update the Attributes, when user is choosing different MaterialIds.
                PropertiesToolsMaterialParams.SetMaterialDefinitionParamsAtts(this, (int) nupMaterialId.Value);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupMaterialId_ValueChanged method threw the exception " + ex.Message);
#endif
            }
        }

        // 2/10/2010
        /// <summary>
        /// EventHandler, used to retrieve the material Id & params for the current
        /// picked InstancedModel. 
        /// </summary>
        private void btnRetrieveData_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // update the given material ID definition
                PropertiesToolsMaterialParams.UpdateMaterialDefinitionParams(this);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnRetrieveData_Click method threw the exception " + ex.Message);
#endif
            }
        }

        #endregion

        #region AbstractBehavior Helper Methods

        // 10/11/2008:          
        /// <summary>
        /// Helper Function: Updates a AbstractBehavior's Attribute, using Reflection.
        /// </summary>
        /// <typeparam name="T">AbstractBehavior Class Type to Update</typeparam>
        /// <param name="propertyName">Property name to Update</param>
        /// <param name="subFieldName">Field name to Update</param>
        /// <param name="behaviorToUpdate">AbstractBehavior Enum Type</param>
        /// <param name="newValue">New Value</param>
        private void UpdateItemBehaviorAttribute<T>(string propertyName, string subFieldName,
                                                    BehaviorsEnum behaviorToUpdate, object newValue)
            where T : AbstractBehavior
        {
            try // 6/22/2010
            {
                if (_resettingValues)
                    return;

                // 8/12/2009
                var sceneItemWithPick = (_currentSceneItem as SceneItemWithPick);

                if (sceneItemWithPick == null) return;

                // Get AbstractBehavior to update
                AbstractBehavior abstractBehavior;

                if (
                    !sceneItemWithPick.ForceBehaviors.Behaviors.TryGetValue((int) behaviorToUpdate, out abstractBehavior))
                    return;

                // Use Reflection to get to Property Type
                var instance = (abstractBehavior as T);
                if (instance != null)
                {
                    Type type = instance.GetType();
                    // Get Property Value
                    PropertyInfo myPropInfo = type.GetProperty(propertyName);
                    object myPropValue = myPropInfo.GetValue(instance, null);

                    // Set Subfield Value in Property
                    FieldInfo myFieldInfo = myPropValue.GetType().GetField(subFieldName);
                    myFieldInfo.SetValue(myPropValue, newValue);
                    // Save Back into Property
                    myPropInfo.SetValue(instance, myPropValue, null);
                }

                // Save AbstractBehavior back into 'behaviors'
                sceneItemWithPick.ForceBehaviors.Behaviors[(int) behaviorToUpdate] = instance;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("UpdateItemBehaviorAttribute method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/11/2008: 
        // Overload Version 1. 
        /// <summary>
        /// Helper Function: Updates a AbstractBehavior's Attribute, using Reflection.
        /// Overload V1: This version only takes a 'Property' name.
        /// </summary>
        /// <typeparam name="T">AbstractBehavior Class Type to Update</typeparam>
        /// <param name="propertyName">Property name to Update</param>        
        /// <param name="behaviorToUpdate">AbstractBehavior Enum Type</param>
        /// <param name="newValue">New Value</param>
        private void UpdateItemBehaviorAttribute<T>(string propertyName, BehaviorsEnum behaviorToUpdate,
                                                    object newValue) where T : AbstractBehavior
        {
            try // 6/22/2010
            {
                if (_resettingValues)
                    return;

                // 8/12/2009
                var sceneItemWithPick = (_currentSceneItem as SceneItemWithPick);

                if (sceneItemWithPick == null) return;

                // Get AbstractBehavior to update
                AbstractBehavior abstractBehavior;

                if (
                    !sceneItemWithPick.ForceBehaviors.Behaviors.TryGetValue((int) behaviorToUpdate, out abstractBehavior))
                    return;

                // Use Reflection to get to Property Type
                var instance = (abstractBehavior as T);
                if (instance != null)
                {
                    Type type = instance.GetType();

                    // Get Property Value
                    PropertyInfo myPropInfo = type.GetProperty(propertyName);

                    // Set Property to New Value
                    myPropInfo.SetValue(instance, newValue, null);
                }

                // Save AbstractBehavior back into 'behaviors'
                sceneItemWithPick.ForceBehaviors.Behaviors[(int) behaviorToUpdate] = instance;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("UpdateItemBehaviorAttribute method threw the exception " + ex.Message);
#endif
            }
        }

        // 10/11/2008       
        /// <summary>
        /// Helper Function: Turns On or Off the given AbstractBehavior.
        /// </summary>
        /// <param name="isChecked">On/Off Boolean</param>
        /// <param name="behavior">AbstractBehavior Enum Type</param>
        private void UpdateItemBehavior(bool isChecked, BehaviorsEnum behavior)
        {
            try // 6/22/2010
            {
                if (_resettingValues)
                    return;

                // 8/12/2009
                var sceneItemWithPick = (_currentSceneItem as SceneItemWithPick);

                if (sceneItemWithPick == null) return;

                if (isChecked)
                {
                    // Add AbstractBehavior
                    // Make sure doesn't already have this AbstractBehavior
                    if (!sceneItemWithPick.ForceBehaviors.Behaviors.ContainsKey((int) behavior))
                        sceneItemWithPick.ForceBehaviors.Add(behavior);
                }
                else
                {
                    // Remove AbstractBehavior
                    sceneItemWithPick.ForceBehaviors.Remove(behavior);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("UpdateItemBehavior method threw the exception " + ex.Message);
#endif
            }
        }

        #endregion
        // ReSharper restore InconsistentNaming     
    }
}