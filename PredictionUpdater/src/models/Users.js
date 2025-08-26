const Sequelize = require('sequelize');
module.exports = function(sequelize, DataTypes) {
  return sequelize.define('Users', {
    Email: {
      type: DataTypes.STRING(450),
      allowNull: false,
      primaryKey: true
    },
    Username: {
      type: DataTypes.STRING(450),
      allowNull: false
    },
    Hashed_password: {
      type: DataTypes.TEXT,
      allowNull: false,
      defaultValue: "(N"
    }
  }, {
    sequelize,
    tableName: 'Users',
    schema: 'dbo',
    timestamps: false,
    indexes: [
      {
        name: "IX_Users_Username",
        unique: true,
        fields: [
          { name: "Username" },
        ]
      },
      {
        name: "PK_Users",
        unique: true,
        fields: [
          { name: "Email" },
        ]
      },
    ]
  });
};
