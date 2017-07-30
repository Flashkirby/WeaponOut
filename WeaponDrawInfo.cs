using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;

namespace WeaponOut
{
    static class WeaponDrawInfo
    {
        internal static DrawData modDraw_WalkCycle(DrawData data, Player p)
        {
            if ((p.bodyFrame.Y / p.bodyFrame.Height >= 7 && p.bodyFrame.Y / p.bodyFrame.Height <= 9)
                || (p.bodyFrame.Y / p.bodyFrame.Height >= 14 && p.bodyFrame.Y / p.bodyFrame.Height <= 16))
            {
                data.position.Y -= 2 * p.gravDir;
            }
            switch (p.bodyFrame.Y / p.bodyFrame.Height)
            {
                case 7: data.position.X -= p.direction * 2; break;
                case 8: data.position.X -= p.direction * 2; break;
                case 9: data.position.X -= p.direction * 2; break;
                case 10: data.position.X -= p.direction * 2; break;
                case 14: data.position.X += p.direction * 2; break;
                case 15: data.position.X += p.direction * 4; break;
                case 16: data.position.X += p.direction * 4; break;
                case 17: data.position.X += p.direction * 2; break;
            }
            return data;
        }
        internal static float rotationWalkCycle(int FrameNum)
        {
            //6 - 19
            //furthest left 9
            //furthest right 16
            float rot = 0;
            switch (FrameNum)
            {
                case 6: rot = -0.6f; break;
                case 7: rot = -0.8f; break;
                case 8: rot = -1; break;
                case 9: rot = -1; break;
                case 10: rot = -0.8f; break;
                case 11: rot = -0.4f; break;
                case 12: rot = 0; break;
                case 13: rot = 0.6f; break;
                case 14: rot = 0.8f; break;
                case 15: rot = 1; break;
                case 16: rot = 1; break;
                case 17: rot = 0.8f; break;
                case 18: rot = 0.4f; break;
                case 19: rot = 0; break;
            }

            return rot;
        }
        internal static void drawGlowLayer(DrawData data, Player drawPlayer, Item heldItem)
        {
            //items that GLOOOW
            if (heldItem.glowMask != -1)
            {
                Color glowLighting = new Microsoft.Xna.Framework.Color(250, 250, 250, heldItem.alpha);
                glowLighting = drawPlayer.GetImmuneAlpha(heldItem.GetAlpha(glowLighting) * drawPlayer.stealth, 0);
                DrawData glowData = new DrawData(
                   Main.glowMaskTexture[heldItem.glowMask],
                   data.position,
                   data.sourceRect,
                   glowLighting,
                   data.rotation,
                   data.origin,
                   data.scale,
                   data.effect,
                   0);

                Main.playerDrawData.Add(glowData);
            }
        }

        internal static DrawData modDraw_HandWeapon(DrawData data, Player p, float length, float width)
        {
            return modDraw_HandWeapon(data, p, length, width, false);
        }
        internal static DrawData modDraw_HandWeapon(DrawData data, Player p, float length, float width, bool isYoyo)
        {
            int playerBodyFrameNum = p.bodyFrame.Y / p.bodyFrame.Height;
            if (isYoyo)
            {
                length /= 2;
                width /= 2;
            }
            if (playerBodyFrameNum < 5) //standing
            {
                data.rotation = (float)(Math.PI * 0.5d * p.direction) * p.gravDir; //rotate 90 clockwise
                data.position += new Vector2((4 - length * 0.1f) * p.direction, (width * 0.3f - 4 + 13) * p.gravDir); //back and down;
                if (isYoyo) data.position.X -= 8 * p.direction;
            }
            else if (playerBodyFrameNum == 5) //jumping
            {
                data.rotation = (float)(Math.PI * -0.25d * p.direction) * p.gravDir; //rotate 90 clockwise
                data.position += new Vector2((width * 0.5f - 8 - length / 2) * p.direction, -14 * p.gravDir); //back and down;
            }
            else //walk cycle base
            {
                data.rotation = (float)(Math.PI * 0.2d * p.direction) * p.gravDir; //rotate 90 clockwise
                data.position += new Vector2((-5 + width * 0.5f) * p.direction, (width * 0.5f - 8 + 14 - length / 2) * p.gravDir);
                data = modDraw_WalkCycle(data, p);
            }
            return data;
        }
        internal static DrawData modDraw_WaistWeapon(DrawData data, Player p, float length)
        {
            float maxFall = 2f;
            if (!ModConf.toggleWaistRotation)
            {
                maxFall = p.velocity.Y * p.gravDir;
                if (p.velocity.Y == 0) maxFall = p.velocity.X * p.direction;
            }
            data.rotation = (float)(Math.PI * 1 + Math.PI * (0.1f + maxFall * 0.01f) * p.direction) * p.gravDir; //rotate just over 180 clockwise
            data.position.X -= (length * 0.5f - 20) * p.direction; //back
            data.position.Y += (14 - maxFall / 2) * p.gravDir; //down
            return data;
        }
        internal static DrawData modDraw_BackWeapon(DrawData data, Player p, float length)
        {
            data.rotation = (float)((Math.PI * 1.1f + Math.PI * (length * -0.001f)) * p.direction) * p.gravDir; //rotate just over 180 clockwise
            data.position.X -= 8 * p.direction; //back
            data.position.Y -= (length * 0.2f - 16) * p.gravDir; //up

            int playerBodyFrameNum = p.bodyFrame.Y / p.bodyFrame.Height;
            if (playerBodyFrameNum == 7 || playerBodyFrameNum == 8 || playerBodyFrameNum == 9
            || playerBodyFrameNum == 14 || playerBodyFrameNum == 15 || playerBodyFrameNum == 16)
            {
                data.position.Y -= 2 * p.gravDir; //up
            }
            return data;
        }
        internal static DrawData modDraw_PoleWeapon(DrawData data, Player p, float length)
        {
            int playerBodyFrameNum = p.bodyFrame.Y / p.bodyFrame.Height;
            if (playerBodyFrameNum < 5) //standing
            {
                data.rotation = (float)(Math.PI * 0.4d - (length * 0.002d)) * p.direction * p.gravDir; //clockwise
                data.position.X += 8 * p.direction; //forward
                data.position.Y += (length * 0.1f + 14) * p.gravDir; //down
            }
            else if (playerBodyFrameNum == 5) //jumping
            {
                data.rotation = (float)(Math.PI * 0.1d + (length * 0.002d)) * p.direction * p.gravDir; //clockwise
                data.position.X += 6 * p.direction; //forward
                data.position.Y -= (10 + length * 0.1f) * p.gravDir; //up
            }
            else //walk cycle base
            {
                data.rotation = (float)(Math.PI * 0.3d) * p.direction * p.gravDir; //rotate 90 clockwise
                data.position.X += 10 * p.direction; //forward
                data.position.Y += (length * 0.1f + 6) * p.gravDir; //down
                data = modDraw_WalkCycle(data, p);
            }
            return data;
        }
        internal static DrawData modDraw_DrillWeapon(DrawData data, Player p, float length)
        {
            int playerBodyFrameNum = p.bodyFrame.Y / p.bodyFrame.Height;

            if (playerBodyFrameNum < 5) //standing
            {
                data.rotation = (float)(Math.PI * 0.1d * p.direction) * p.gravDir;
                data.position.X += (8 - (length * 0.1f)) * p.direction; //back
                data.position.Y += 14 * p.gravDir; //down

            }
            else if (playerBodyFrameNum == 5) //jumping
            {
                data.rotation = (float)(Math.PI * -0.5d * p.direction) * p.gravDir;
                data.position.X -= 7 * p.direction; //back
                data.position.Y -= (24 - (length * 0.2f)) * p.gravDir; //down

            }
            else //walk cycle base
            {
                data.rotation = (float)(Math.PI * 0.05d * p.direction) * p.gravDir;
                data.position.X += (10 - (length * 0.1f)) * p.direction; //back
                data.position.Y += 10 * p.gravDir; //down
                data = modDraw_WalkCycle(data, p);
            }
            return data;
        }
        internal static DrawData modDraw_ForwardHoldWeapon(DrawData data, Player p, float width)
        {
            int playerBodyFrameNum = p.bodyFrame.Y / p.bodyFrame.Height;

            if (playerBodyFrameNum < 5) //standing
            {
                data.rotation = (float)(Math.PI * 0.4d) * p.direction * p.gravDir;
                data.position += new Vector2(-6 * p.direction, (16 - width * 0.1f) * p.gravDir);
            }
            else if (playerBodyFrameNum == 5) //jumping
            {
                data.rotation = (float)(Math.PI * -0.35d) * p.direction * p.gravDir;
                data.position += new Vector2(-8 * p.direction, (width * 0.1f - 12) * p.gravDir);
            }
            else //walk cycle base
            {
                data.rotation = (float)(Math.PI * 0.2d) * p.direction * p.gravDir;
                data.position += new Vector2(-2 * p.direction, (10 - width * 0.1f) * p.gravDir);
                data = modDraw_WalkCycle(data, p);
            }
            return data;
        }
        internal static DrawData modDraw_AimedWeapon(DrawData data, Player p, float length)
        {
            int playerBodyFrameNum = p.bodyFrame.Y / p.bodyFrame.Height;

            if (playerBodyFrameNum < 5) //standing
            {
                data.rotation = (float)(Math.PI * 0.5d * p.direction) * p.gravDir; //rotate 90 clockwise
                data.position += new Vector2(-2 * p.direction, (length * 0.5f) * p.gravDir); //back and down;
            }
            else if (playerBodyFrameNum == 5) //jumping
            {
                data.rotation = (float)(Math.PI * -0.75d * p.direction) * p.gravDir; //rotate 90 clockwise
                data.position += new Vector2((-10 - length * 0.2f) * p.direction, (10 - length / 2) * p.gravDir); //back and down;
            }
            else //walk cycle base
            {
                data.rotation = (float)(Math.PI * 0.1d * p.direction) * p.gravDir; //rotate 90 clockwise
                data.position += new Vector2(6 * p.direction, 8 * p.gravDir);
                data = modDraw_WalkCycle(data, p);
            }
            return data;
        }
        internal static DrawData modDraw_HeavyWeapon(DrawData data, Player p, float width)
        {
            int playerBodyFrameNum = p.bodyFrame.Y / p.bodyFrame.Height;
            if (playerBodyFrameNum < 5 || (playerBodyFrameNum == 10 && p.velocity.X == 0)) //standing
            {
                data.rotation = (float)(Math.PI * 0.005d) * width * p.direction * p.gravDir;
                data.position.X += 4 * p.direction; //forward
                data.position.Y += (width * 0.1f + 6) * p.gravDir; //down
            }
            else if (playerBodyFrameNum == 5) //jumping
            {
                data.rotation = (float)(Math.PI * -0.002d) * width * p.direction * p.gravDir;
                data.position.X += 2 * p.direction; //forward
                data.position.Y -= (width * 0.1f + 10) * p.gravDir; //up
            }
            else //walk cycle base
            {
                data.rotation = (float)(Math.PI * 0.008d) * (width * 0.2f + rotationWalkCycle(playerBodyFrameNum) * 6) * p.direction * p.gravDir;
                data.position.X += 8 * p.direction; //forward
                data.position.Y += width * 0.15f * p.gravDir; //down
                data = modDraw_WalkCycle(data, p);
            }
            return data;
        }
        internal static DrawData modDraw_MagicWeapon(DrawData data, Player p, float length)
        {
            int playerBodyFrameNum = p.bodyFrame.Y / p.bodyFrame.Height;
            if (playerBodyFrameNum < 5) //standing
            {
                data.rotation = (float)(Math.PI * 0.2d) * p.direction * p.gravDir;
                data.position.X += (length * 0.1f + 4) * p.direction; //forward
                data.position.Y += (length * 0.1f + 6) * p.gravDir; //down
            }
            else if (playerBodyFrameNum == 5) //jumping
            {
                data.rotation = (float)(Math.PI * -0.45d - (length * 0.002d)) * p.direction * p.gravDir; //clockwise
                data.position.X -= (length * 0.1f + 16) * p.direction; //back
                data.position.Y -= (length * 0.16f + 14) * p.gravDir; //up
            }
            else //walk cycle base
            {
                data.rotation = (float)(Math.PI * -0.2d - (length * 0.002d)) * p.direction * p.gravDir; //anticlockwise
                data.position.X -= 2 * p.direction; //back
                data.position.Y -= (length * 0.4f - 12) * p.gravDir; //up
                data = modDraw_WalkCycle(data, p);
            }
            return data;
        }
    }
}
