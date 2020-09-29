namespace RedisClient
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblInfo = new System.Windows.Forms.Label();
            this.btnisConnected = new System.Windows.Forms.Button();
            this.btnStartRedis = new System.Windows.Forms.Button();
            this.btnKillRedis = new System.Windows.Forms.Button();
            this.btnGetKeys = new System.Windows.Forms.Button();
            this.lblTreatKeysPnlBack = new System.Windows.Forms.Label();
            this.lblOutputScrlPanBack = new System.Windows.Forms.Label();
            this.btnNewKey = new System.Windows.Forms.Button();
            this.txtUrlRedisServer = new System.Windows.Forms.TextBox();
            this.lblSemicolon = new System.Windows.Forms.Label();
            this.txtPortRedisServer = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblInfo
            // 
            this.lblInfo.AutoEllipsis = true;
            this.lblInfo.Location = new System.Drawing.Point(12, 42);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(357, 59);
            this.lblInfo.TabIndex = 1;
            this.lblInfo.Text = "label1";
            // 
            // btnisConnected
            // 
            this.btnisConnected.Location = new System.Drawing.Point(376, 12);
            this.btnisConnected.Name = "btnisConnected";
            this.btnisConnected.Size = new System.Drawing.Size(68, 23);
            this.btnisConnected.TabIndex = 2;
            this.btnisConnected.Text = "is running?";
            this.btnisConnected.UseVisualStyleBackColor = true;
            // 
            // btnStartRedis
            // 
            this.btnStartRedis.Location = new System.Drawing.Point(377, 42);
            this.btnStartRedis.Name = "btnStartRedis";
            this.btnStartRedis.Size = new System.Drawing.Size(67, 23);
            this.btnStartRedis.TabIndex = 3;
            this.btnStartRedis.Text = "start";
            this.btnStartRedis.UseVisualStyleBackColor = true;
            // 
            // btnKillRedis
            // 
            this.btnKillRedis.Location = new System.Drawing.Point(451, 11);
            this.btnKillRedis.Name = "btnKillRedis";
            this.btnKillRedis.Size = new System.Drawing.Size(27, 23);
            this.btnKillRedis.TabIndex = 4;
            this.btnKillRedis.Text = "kill";
            this.btnKillRedis.UseVisualStyleBackColor = true;
            // 
            // btnGetKeys
            // 
            this.btnGetKeys.Location = new System.Drawing.Point(376, 86);
            this.btnGetKeys.Name = "btnGetKeys";
            this.btnGetKeys.Size = new System.Drawing.Size(68, 23);
            this.btnGetKeys.TabIndex = 6;
            this.btnGetKeys.Text = "Keys";
            this.btnGetKeys.UseVisualStyleBackColor = true;
            // 
            // lblTreatKeysPnlBack
            // 
            this.lblTreatKeysPnlBack.Location = new System.Drawing.Point(534, 9);
            this.lblTreatKeysPnlBack.Name = "lblTreatKeysPnlBack";
            this.lblTreatKeysPnlBack.Size = new System.Drawing.Size(453, 72);
            this.lblTreatKeysPnlBack.TabIndex = 7;
            this.lblTreatKeysPnlBack.Text = "label1";
            // 
            // lblOutputScrlPanBack
            // 
            this.lblOutputScrlPanBack.Location = new System.Drawing.Point(534, 96);
            this.lblOutputScrlPanBack.Name = "lblOutputScrlPanBack";
            this.lblOutputScrlPanBack.Size = new System.Drawing.Size(453, 426);
            this.lblOutputScrlPanBack.TabIndex = 9;
            this.lblOutputScrlPanBack.Text = "label1";
            // 
            // btnNewKey
            // 
            this.btnNewKey.Location = new System.Drawing.Point(377, 116);
            this.btnNewKey.Name = "btnNewKey";
            this.btnNewKey.Size = new System.Drawing.Size(67, 23);
            this.btnNewKey.TabIndex = 10;
            this.btnNewKey.Text = "new key";
            this.btnNewKey.UseVisualStyleBackColor = true;
            // 
            // txtUrlRedisServer
            // 
            this.txtUrlRedisServer.Location = new System.Drawing.Point(13, 9);
            this.txtUrlRedisServer.Name = "txtUrlRedisServer";
            this.txtUrlRedisServer.Size = new System.Drawing.Size(204, 20);
            this.txtUrlRedisServer.TabIndex = 11;
            this.txtUrlRedisServer.Text = "localhost";
            // 
            // lblSemicolon
            // 
            this.lblSemicolon.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lblSemicolon.Location = new System.Drawing.Point(217, 5);
            this.lblSemicolon.Name = "lblSemicolon";
            this.lblSemicolon.Size = new System.Drawing.Size(18, 23);
            this.lblSemicolon.TabIndex = 12;
            this.lblSemicolon.Text = ":";
            // 
            // txtPortRedisServer
            // 
            this.txtPortRedisServer.Location = new System.Drawing.Point(234, 9);
            this.txtPortRedisServer.Name = "txtPortRedisServer";
            this.txtPortRedisServer.Size = new System.Drawing.Size(53, 20);
            this.txtPortRedisServer.TabIndex = 13;
            this.txtPortRedisServer.Text = "6379";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(999, 531);
            this.Controls.Add(this.txtPortRedisServer);
            this.Controls.Add(this.lblSemicolon);
            this.Controls.Add(this.txtUrlRedisServer);
            this.Controls.Add(this.btnNewKey);
            this.Controls.Add(this.lblOutputScrlPanBack);
            this.Controls.Add(this.lblTreatKeysPnlBack);
            this.Controls.Add(this.btnGetKeys);
            this.Controls.Add(this.btnKillRedis);
            this.Controls.Add(this.btnStartRedis);
            this.Controls.Add(this.btnisConnected);
            this.Controls.Add(this.lblInfo);
            this.Name = "MainForm";
            this.Text = "Redis Manager";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button btnisConnected;
        private System.Windows.Forms.Button btnStartRedis;
        private System.Windows.Forms.Button btnKillRedis;
        private System.Windows.Forms.Button btnGetKeys;
        private System.Windows.Forms.Label lblTreatKeysPnlBack;
        private System.Windows.Forms.Label lblOutputScrlPanBack;
        private System.Windows.Forms.Button btnNewKey;
        private System.Windows.Forms.TextBox txtUrlRedisServer;
        private System.Windows.Forms.Label lblSemicolon;
        private System.Windows.Forms.TextBox txtPortRedisServer;
    }
}

